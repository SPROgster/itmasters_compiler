﻿using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;
using MiddleEnd;
using DefUse;
using CleanDead;

namespace SimpleCompiler
{
    using BaseBlock = LinkedList<CodeLine>;
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

                    Fold.fold(ref gcv.Code); // Вызов сворачивания констант и алг тождеств

                    //Причёсываем метки
                    //var Iterator = gcv.Code.First;
                    //while (Iterator != null)
                    //{
                    //    if (Iterator.Value.First == null && Iterator.Next != null)
                    //    {
                    //        Iterator.Next.Value.Label = Iterator.Value.Label;
                    //        Iterator = Iterator.Next;
                    //        gcv.Code.Remove(Iterator.Previous);
                    //    }
                    //    else
                    //        Iterator = Iterator.Next;
                    //}

                    Correct3AddressCode.RemoveEmptyLabel(ref gcv.Code);

                    Console.WriteLine();
                    Console.WriteLine("Трёхадресный код:");
                    foreach (var ln in gcv.Code)
                        Console.WriteLine(ln);

                    ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
                    List<BaseBlock> l=new List<BaseBlock>(CFG.GetBlocks());
                    Console.WriteLine(DeadOrAlive.IsAlive(l[0],"a",1).ToString());
                    Console.WriteLine("Граф построен!");

                    Console.WriteLine("\nУдаляем мёртвый код:");
                    LinkedList<BaseBlock> blocks = CFG.GetBlocks();
                    for (var p = blocks.First; p != null; p = p.Next)
                    {
                        CleanDead.CleanDead.cleanBlock(p.Value);
                        for (var r = p.Value.First; r != null; r = r.Next)
                            Console.WriteLine(r.Value.ToString());
                    }
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
