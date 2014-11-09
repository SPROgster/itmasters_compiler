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
            string FileName = @"..\..\_TestTexts\ReachingDefsTest.txt";
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
                        //Устранение Nop-ов и коррекция меток
                        gcv.RemoveEmptyLabels();
                        // Вызов сворачивания констант и алг тождеств
                        Fold.fold(ref gcv.Code);
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
                        Console.WriteLine(l[1]);
                        Console.WriteLine("---------------------------------");
                        //Тест Оптимизация общих подвыражений
                        l[1].optimization_CSE_inBBl();
                        Console.WriteLine(l[1]);
                        Console.WriteLine(DeadOrAlive.IsAlive(l[0], "a", 1).ToString());
                        //Проверяем алгоритм поиска достигающих определений
                        Console.WriteLine();
                        ReachingDefsAlgorithm RDA = new ReachingDefsAlgorithm(CFG);
                        var RDAResult = RDA.Apply();
                        foreach (var block in RDAResult.Item1.Keys)
                            if (block != CFG.GetStart() && block != CFG.GetEnd())
                            {
                                Console.WriteLine(block);
                                Console.WriteLine("In:\t" + RDAResult.Item1[block].ToString().Replace("True", "1").Replace("False", "0"));
                                Console.WriteLine("Out:\t" + RDAResult.Item2[block].ToString().Replace("True", "1").Replace("False", "0"));
                            }
                        //Проверяем алгоритм поиска живых переменных
                        Console.WriteLine();
                        AliveVarsAlgorithm AVA = new AliveVarsAlgorithm(CFG);
                        var AVAResult = AVA.Apply();
                        foreach (var block in AVAResult.Item1.Keys)
                            if (block != CFG.GetStart() && block != CFG.GetEnd())
                            {
                                Console.WriteLine(block);
                                Console.WriteLine("In:\t" + AVAResult.Item1[block]);
                                Console.WriteLine("Out:\t" + AVAResult.Item2[block]);
                            }
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
