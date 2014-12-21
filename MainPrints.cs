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
        //Функция для вывода заданного графа потока управления
        public static void PrintCFG(ControlFlowGraph cfg)
        {
            Print("Граф потока управления:");           
            foreach (BaseBlock block in cfg.GetBlocks())
            {
                if (block != cfg.GetStart() && block != cfg.GetEnd())
                {
                    Console.WriteLine("--- Блок {0} ---", block.nBlock);
                    Console.WriteLine(block);
                    Console.WriteLine();
                    Console.WriteLine("Input:\t" + String.Join(" ",cfg.GetInputs(block).
                        Select(e=>e.nBlock.ToString())));
                    Console.WriteLine("Output:\t" + String.Join(" ", cfg.GetOutputs(block).
                        Select(e => e.nBlock.ToString())));
                    Console.WriteLine("----------------");                    
                }
            }
        }

        public static void PrintCode(BlockNode root)
        {
            Print("Восстановленный исходный код:");
            var pp = new PrettyPrintVisitor();
            root.Visit(pp);
            Console.WriteLine(pp.Text);
        }

        public static void PrintSymbolTable()
        {
           Print("Таблица символов:");
            foreach (var rec in SymbolTable.vars)
                Console.WriteLine(rec);
        }

        public static void PrintEnd()
        {
            if (Output != null)
            {
                Output.Close();
                StreamWriter Std = new StreamWriter(Console.OpenStandardOutput(),
                    System.Text.Encoding.GetEncoding(866));
                Std.AutoFlush = true;
                Console.SetOut(Std);
            }
            Console.Write("Для завершения работы программы нажмите Enter...");
            Console.ReadLine();
        }

        public static void Print(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg);
            Console.WriteLine();
        }

        /////////////////////////////////////////////////////////////////////////

        public delegate void RunAnalysis(ControlFlowGraph CFG); 

        public static void RunAvailableExpressions(ControlFlowGraph CFG)
        {
            Print("Доступные выражения:");  
            AvailableExprsAlgorithm AEA = new AvailableExprsAlgorithm(CFG);
            var AEAResult = AEA.Apply();
            foreach (var block in AEAResult.Item1.Keys)
                if (block != CFG.GetStart() && block != CFG.GetEnd())
                {
                    Console.WriteLine(block);
                    Console.WriteLine("In:\t" + AEAResult.Item1[block]);
                    Console.WriteLine("Out:\t" + AEAResult.Item2[block]);
                }
        }

        public static void RunDeadOrAlive(ControlFlowGraph CFG)
        {
            Print("Активные переменные:");  
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

        public static void RunDominatorTree(ControlFlowGraph CFG)
        {
            Print("Дерево доминаторов:");  
            DominatorsTree DTree = new DominatorsTree(CFG);
            Stack<Pair<DominatorsTree.TreeNode<BaseBlock>,int>> Path =
                new Stack<Pair<DominatorsTree.TreeNode<BaseBlock>, int>>();
            Path.Push(new Pair<DominatorsTree.TreeNode<BaseBlock>,int>(DTree.Root,0));
            int Depth = 0;
            while(Path.Count>0)
            {
                var Top = Path.Peek();
                if (Top.snd != Top.fst.Items.Count)
                {
                    Path.Push(new Pair<DominatorsTree.TreeNode<BaseBlock>, int>(
                        Top.fst.Items.ElementAt(Top.snd), 0));
                    Top.snd += 1;
                    Depth += 1;
                }
                else
                {
                    for (int i = 0; i < Depth; ++i)
                        Console.Write("  ");
                    Console.WriteLine(Top.fst.Value.nBlock);
                    Path.Pop();
                    Depth -= 1;
                }
            }
        }

        public static void RunReachingDefinitions(ControlFlowGraph CFG)
        {
            Print("Достигающие определения:");  
            ReachingDefsAlgorithm RDA = new ReachingDefsAlgorithm(CFG);
            var RDAResult = RDA.Apply();
            foreach (var block in RDAResult.Item1.Keys)
                if (block != CFG.GetStart() && block != CFG.GetEnd())
                {
                    Console.WriteLine(block);
                    Console.WriteLine("In:\t" + RDAResult.Item1[block].ToString().Replace("True", "1").Replace("False", "0"));
                    Console.WriteLine("Out:\t" + RDAResult.Item2[block].ToString().Replace("True", "1").Replace("False", "0"));
                }
        }

        public static void RunSpanningTree(ControlFlowGraph CFG)
        {
            Print("Остовное дерево:");  
            var spTree = new SpanningTree(CFG);
            spTree.Print();
            spTree.PrintEdges(); 
        }
    }
}