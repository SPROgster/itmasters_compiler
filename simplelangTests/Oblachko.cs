using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleLang.Visitors;
using SimpleParser;
using SimpleScanner;
using System;
using System.Collections.Generic;
using System.IO;

namespace simplelangTests
{
    [TestClass]
    public class Oblachko
    {
        [TestMethod]
        public void Fold_1_2()
        {
            string FileName = @"..\..\Test5.txt";
            string Text = File.ReadAllText(FileName);
            Scanner scanner = new Scanner();
            scanner.SetSource(Text, 0);
            Parser parser = new Parser(scanner);
            var b = parser.Parse();
            var sne = new CheckVariablesVisitor();
            parser.root.Visit(sne);
            GenCodeVisitor gcv = new GenCodeVisitor();
            parser.root.Visit(gcv);
            gcv.RemoveEmptyLabels();
            ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
            SimpleCompilerMain.PrintCFG(CFG);
            Fold.fold(ref gcv.Code);
            SimpleCompilerMain.PrintCFG(CFG);
            List<CodeLine> myList = new List<CodeLine>(gcv.Code);
            Assert.AreEqual("5", myList[3].Second);
            Assert.AreEqual("-2", myList[4].Second);
            Assert.AreEqual("4", myList[5].Second);
        }
        [TestMethod]
        public void RDA_2_6()
        {
            string FileName = @"..\..\Test3.txt";
            string Text = File.ReadAllText(FileName);
            Scanner scanner = new Scanner();
            scanner.SetSource(Text, 0);
            Parser parser = new Parser(scanner);
            var b = parser.Parse();
            var sne = new CheckVariablesVisitor();
            parser.root.Visit(sne);
            GenCodeVisitor gcv = new GenCodeVisitor();
            parser.root.Visit(gcv);
            gcv.RemoveEmptyLabels();
            ControlFlowGraph CFG = new ControlFlowGraph(gcv.Code);
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
            List<BaseBlock> bl = new List<BaseBlock>(CFG.GetBlocks());
            BitSet resultin = RDAResult.Item1[bl[3]];
            BitSet rightin = new BitSet(new bool[] { true, true, true, false, true, true, true });
            Assert.IsTrue(resultin.Equals(rightin));
        }
    }
}