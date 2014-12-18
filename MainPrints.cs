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