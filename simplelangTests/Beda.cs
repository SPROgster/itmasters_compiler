using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Visitors;
using SimpleLang.Analysis;
using SimpleCompiler;
using System.Collections.Generic;

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
            SimpleCompilerMain.PrintCFG(CFG);
            List<CodeLine> bl = new List<CodeLine>(CFG.GetBlocks().First.Next.Value.Code);
            for (int i = 0; i < bl.Count; i++)
            {
                Console.WriteLine(bl[i]+ " "+ DeadOrAlive.IsDead(CFG.GetBlocks().First.Next.Value, bl[i].First, i));
            }
              
            bool alive =
                DeadOrAlive.IsAliveBeforeLine(CFG.GetBlocks().First.Next.Value, "i", 0) &&
                DeadOrAlive.IsAliveAfterLine(CFG.GetBlocks().First.Next.Value, "i", 0);
            Assert.IsFalse(alive);
        }
    }
}
