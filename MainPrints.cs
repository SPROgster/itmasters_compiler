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
            Console.WriteLine();
            int j = 0; // # блока
            foreach (BaseBlock block in cfg.GetBlocks())
            {
                Console.WriteLine("--- Блок {0} ---", j);
                Console.WriteLine(block);
                ++j;
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
        }
    }
}