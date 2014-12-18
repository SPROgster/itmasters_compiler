using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using SimpleScanner;
using SimpleParser;

using SimpleLang.Visitors;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleLang.Analysis;
using SimpleLang.CodeGenerator;

namespace SimpleCompiler
{
    public partial class SimpleCompilerMain
    {
        public static string Help = @"
Запуск программы: simplelang.exe <имя_компилируемого_файла> [<опции>]
Доступные опции:
    -p - выводить содержимое структур внутреннего представления, получаемых в процессе компиляции
    -lo <список_номеров_без_пробелов> - применить заданные внутриблочные (локальные) оптимизации
    -go <список_номеров_без_пробелов> - применить заданные межблочные (глобальные) оптимизации
    -a <список_номеров_без_пробелов> - провести заданный анализ программы
";

        public static void Main(string[] cmd)
        {
            //Создаём директорию для вывода
            if (!Directory.Exists(BinOutputDirectory))
                Directory.CreateDirectory(BinOutputDirectory);
            bool ShouldPrint = cmd.Contains("-p");
            //Получаем список локальных оптимизаций
            LocalOptimization[] LocalOp = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(t => t.GetTypes()).
                Where(t => t.IsClass &&
                t.Namespace == @"SimpleLang.Optimizations" &&
                t.GetInterfaces().Contains(typeof(LocalOptimization))).
                Select(e => (LocalOptimization)e.GetConstructor(new Type[0]).Invoke(new object[0])).
                ToArray();
            //Получаем список глобальных оптимизаций
            GlobalOptimization[] GlobalOp = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(t => t.GetTypes()).
                Where(t => t.IsClass &&
                t.Namespace == @"SimpleLang.Optimizations" &&
                t.GetInterfaces().Contains(typeof(GlobalOptimization))).
                Select(e => (GlobalOptimization)e.GetConstructor(new Type[0]).Invoke(new object[0])).
                ToArray();
            //Получаем список методов для запуска анализов
            MethodInfo[] Analyzes = typeof(SimpleCompilerMain).GetMethods().
                Where(e => e.IsStatic && e.Name.StartsWith("Run")).
                ToArray();
            //Приступаем к анализу командной строки
            try
            {
                //Если нет параметров, выводим справку
                if (cmd.Length == 0)
                {
                    ShouldPrint = true;
                    Console.WriteLine(Help);
                    Console.WriteLine("Доступные внутриблочные оптимизации:");
                    for (int i = 0; i < LocalOp.Length; ++i)
                        Console.WriteLine(i + " - " + LocalOp[i].GetType().Name);
                    Console.WriteLine("Доступные межблочные оптимизации:");
                    for (int i = 0; i < GlobalOp.Length; ++i)
                        Console.WriteLine(i + " - " + GlobalOp[i].GetType().Name);
                    Console.WriteLine("Доступные анализы:");
                    for (int i = 0; i < Analyzes.Length; ++i)
                        Console.WriteLine(i + " - " + Analyzes[i].Name.Substring(3));
                }
                else
                {
                    //Если есть файл, который надо компилировать
                    if (File.Exists(cmd[0]))
                    {
                        //Проводим лексический и синтаксический анализ, получаем дерево
                        BlockNode Root = SyntaxAnalysis(cmd[0]);
                        if (ShouldPrint)
                        {
                            Print("Синтаксический анализ завершён.");
                            PrintCode(Root);
                        }
                        //Если всё хорошо и семантический анализ тоже прошёл успешно
                        if(Root!=null && SemanticAnalysis(Root))
                        {
                            //Строим граф потока управления
                            var CFG = BuildCFG(Root);
                            if (ShouldPrint)
                            {
                                Print("Семантический анализ завершён.");
                                PrintSymbolTable();
                                PrintCFG(CFG); 
                            }
                            //Определяем, какие оптимизации и анализы нас попросили применить
                            int[] LOpIndex = null;
                            int[] GOpIndex = null;
                            int[] AIndex = null;
                            for(int i=1;i<cmd.Length;++i)
                                if (cmd[i] != "-p")
                                {
                                    var Inds = cmd[i + 1].Select(e => int.Parse(e.ToString())).Distinct();
                                    switch (cmd[i])
                                    {
                                        case "-lo":
                                            LOpIndex = Inds.ToArray();
                                            ++i;
                                            break;
                                        case "-go":
                                            GOpIndex = Inds.ToArray();
                                            ++i;
                                            break;
                                        case "-a":
                                            AIndex = Inds.ToArray();
                                            ++i;
                                            break;
                                    }
                                }
                            //Проверяем, надо ли провести какие-то анализы
                            if (AIndex != null && ShouldPrint)
                                foreach (int ind in AIndex)
                                    Analyzes[ind].Invoke(null, new object[]{CFG});
                            //Если надо применять хоть какую-то оптимизацию
                            if (GOpIndex != null || LOpIndex != null)
                            {
                                if (GOpIndex == null)
                                    GOpIndex = new int[0];
                                if (LOpIndex == null)
                                    LOpIndex = new int[0];
                                bool WasOptimized = true;
                                //Пока что-то меняется
                                while (WasOptimized)
                                {
                                    WasOptimized = false;
                                    //Применяем внутриблочные оптимизации
                                    foreach (BaseBlock block in CFG.GetBlocks())
                                        for (int i = 0; i < LOpIndex.Length; ++i)
                                            if (LocalOp[LOpIndex[i]].Optimize(block))
                                                i = -1;
                                    for (int i = 0; i < GOpIndex.Length; ++i)
                                        if (GlobalOp[GOpIndex[i]].Optimize(CFG))
                                        {
                                            WasOptimized = true;
                                            break;
                                        }
                                }
                                if (ShouldPrint)
                                {
                                    Print("Оптимизация завершена.");
                                    PrintCFG(CFG);
                                }
                            }
                            //Генерируем исполняемый код
                            ILAsm gen = new ILAsm();
                            gen.compileExe(CFG, BinOutputDirectory);
                            if(ShouldPrint)
                                Print("Исполняемый файл сгенерирован.");
                        }
                    }
                    else
                        Console.WriteLine("Файл " + cmd[0] + " не найден, работа программы прервана.");
                }
            }
            catch (Exception e)
            {
                Print(e.ToString());
                Console.Write("Для завершения работы программы нажмите Enter...");
                Console.ReadLine();
            }
            if(ShouldPrint)
            {
                Console.Write("Для завершения работы программы нажмите Enter...");
                Console.ReadLine();
            }
        }

    }
}
