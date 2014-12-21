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
        public static string FileName = "../../_TestTexts/optCseTest.txt";
        public static string BinOutputDirectory = "../Compiled/";

        //Синтаксический анализ
        public static BlockNode SyntaxAnalysis(string filename)
        {
            FileName = filename;
            string Text = File.ReadAllText(filename);
            Scanner scanner = new Scanner();
            scanner.SetSource(Text, 0);
            Parser parser = new Parser(scanner);
            var b = parser.Parse();
            if (!b)
                throw new Exception("Ошибка на этапе синтаксического анализа, работа программы прервана.");
            return parser.root;
        }

        //Семантический анализ по заданному дереву программы
        public static bool SemanticAnalysis(BlockNode root)
        {
            SymbolTable.Reset();
            var sne = new CheckSemanticsVisitor();
            root.Visit(sne);
            if (sne.Errors.Count > 0)
            {
                Console.WriteLine("Ошибки на этапе семантического анализа, работа программы прервана.");
                foreach (var err in sne.Errors)
                    Console.WriteLine(err);
                return false;
            }
            return true;
        }

        //Функция для построения графа базовых блоков по синтаксическому дереву
        public static ControlFlowGraph BuildCFG(BlockNode root)
        {
            //Генерируем трёхадресный код
            GenCodeVisitor gcv = new GenCodeVisitor();
            root.Visit(gcv);
            //Устранение Nop-ов и коррекция меток
            gcv.RemoveEmptyLabels();
            //Строим граф базовых блоков
            ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
            return CFG;
        }
    }
}