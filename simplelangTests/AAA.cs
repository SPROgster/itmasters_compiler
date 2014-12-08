using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleCompiler;
using SimpleLang.Analysis;

namespace simplelangTests
{
    [TestClass]
    public class AAA
    {
        [TestMethod]
        public void CleanDead_1_5()
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
            SimpleCompilerMain.PrintCFG(CFG);
            CleanDead cl = new CleanDead();
            cl.Optimize(CFG.GetBlocks().First.Next.Value);
            SimpleCompilerMain.PrintCFG(CFG);
            Assert.AreEqual(6, CFG.GetBlocks().First.Next.Value.Code.Count);
        }
    }
}
