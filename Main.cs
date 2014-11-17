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
            string FileName = @"..\..\_TestTexts\optCseTest.txt";
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

                    var pp = new PrettyPrintVisitor();
                    parser.root.Visit(pp);
                    Console.WriteLine(pp.Text);
                    Console.WriteLine("---------------------------------");
                    //Отрабатывают визиторы, проверяющие наличие ошибок
                    var sne = new CheckVariablesVisitor();
                    parser.root.Visit(sne);
                    foreach (var err in sne.Errors)
                        Console.WriteLine(err);
                    if (sne.Errors.Count == 0)
                    {
                        //Генерируем трёхадресный код
                        GenCodeVisitor gcv = new GenCodeVisitor();
                        parser.root.Visit(gcv);
                        // Устранение Nop-ов и коррекция меток
                        gcv.RemoveEmptyLabels();
                        // Устранение лишних временных переменных
                        gcv.RemoveTmpVariables();
                        // Вызов сворачивания констант и алг тождеств
                        //Fold.fold(ref gcv.Code);
                        //
                        Console.WriteLine("Трёхадресный код:");
                        foreach (var ln in gcv.Code)
                            Console.WriteLine(ln);
                        Console.WriteLine("---------------------------------");
                        //Строим граф базовых блоков
                        ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
                        Console.WriteLine("Граф ББ построен!");                        
                        Console.WriteLine("c оптимизацией общих подвыражений в каждом блоке:");
                        int i = 0; // # блока
                        foreach (var block in CFG.GetBlocks())
                        {
                            CSE.cseOptimization(block);
                            Console.WriteLine("--- Блок {0} ---", i);
                            Console.WriteLine(block);
                            ++i;
                            //Console.WriteLine("---------------------------------");                            
                        }
                        Console.WriteLine("---------------------------------");
                       
                    }
                    else
                        Console.WriteLine("Исправьте Ваш кривой код!");
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
            Console.Write("Для завершения работы программы нажмите Enter...");
            Console.ReadLine();
        }

    }
}
