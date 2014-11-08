using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;

using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleLang.Analysis;

namespace SimpleCompiler
{
    public class SimpleCompilerMain
    {
        public static void Main()
        {
            string FileName = @"..\..\b2.txt";
            try
            {
                string Text = File.ReadAllText(FileName);

                Scanner scanner = new Scanner();
                scanner.SetSource(Text, 0);

                Parser parser = new Parser(scanner);

                var b = parser.Parse();
                if (!b)
                    Console.WriteLine("Ошибка");
                else
                {
                    Console.WriteLine("Синтаксическое дерево построено");

                    //var avis = new AssignCountVisitor();
                    //parser.root.Visit(avis);
                    //Console.WriteLine("Количество присваиваний = {0}", avis.Count);
                    //Console.WriteLine("-------------------------------");

                    var pp = new PrettyPrintVisitor();
                    parser.root.Visit(pp);
                    Console.WriteLine(pp.Text);

                    //var vr = new VariableRenameVisitor();
                    //parser.root.Visit(vr);
                    //pp.Text = "";
                    //parser.root.Visit(pp);
                    //Console.WriteLine(pp.Text);

                    //Отрабатывают визиторы, проверяющие наличие ошибок
                    var sne = new CheckVariablesVisitor();
                    parser.root.Visit(sne);
                    foreach (var err in sne.Errors)
                        Console.WriteLine(err);

                    //Генерируем трёхадресный код
                    GenCodeVisitor gcv = new GenCodeVisitor();
                    parser.root.Visit(gcv);
                    // Вызов сворачивания констант и алг тождеств
                    Fold.fold(ref gcv.Code);
                    //Устранение Nop-ов и коррекция меток
                    Correct3AddressCode.RemoveEmptyLabel(ref gcv.Code);
                    //Выводим то, что получилось
                    Console.WriteLine();
                    Console.WriteLine("Трёхадресный код:");
                    foreach (var ln in gcv.Code)
                        Console.WriteLine(ln);
                    //Строим граф базовых блоков
                    ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
                    Console.WriteLine("Граф построен!");
                    //Демонстрируем проверку живучести переменной
                    List<BaseBlock> l = new List<BaseBlock>(CFG.GetBlocks());
                    Console.WriteLine(DeadOrAlive.IsAlive(l[0],"a",1).ToString());
                    //Проверяем алгоритм поиска достигающих определений
                    Console.WriteLine();
                    ReachingDefsAlgorithm RDA = new ReachingDefsAlgorithm(CFG);
                    var Result = RDA.Apply();
                    Console.WriteLine("'In's:");
                    foreach (var elem in Result.Item1)
                        Console.WriteLine(elem.ToString().Replace("True","1").Replace("False","0"));
                    Console.WriteLine("'Out's:");
                    foreach(var elem in Result.Item1)
                        Console.WriteLine(elem.ToString().Replace("True", "1").Replace("False", "0"));
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл {0} не найден", FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e);
            }

            Console.ReadLine();
        }

    }
}
