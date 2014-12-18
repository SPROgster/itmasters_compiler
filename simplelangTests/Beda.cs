using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Visitors;
using SimpleLang.Analysis;

namespace simplelangTests
{
    [TestClass]
    public class Beda
    {
        [TestMethod]
        public void DeadOrAlive_1_1()
        {
            string FileName = @"..\..\Test2.txt";
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
            Assert.IsFalse(DeadOrAlive.IsAlive(CFG.GetBlocks().First.Next.Value, "k", 1));
        }
    }
}
