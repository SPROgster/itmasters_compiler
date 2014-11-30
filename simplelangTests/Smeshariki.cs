using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using System.Collections.Generic;

namespace simplelangTests
{
    [TestClass]
    public class Smeshariki
    {
        [TestMethod]
        public void TestCSE()
        {
            string FileName = @"..\..\Test4.txt";
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
            CSE cse = new CSE();            
            List<BaseBlock> myList = new List<BaseBlock>(CFG.GetBlocks());
            cse.Optimize(myList[1]);
            string f = myList[1].Code.First.Value.First;
            Assert.AreEqual(true, f.Equals("_t4"));
        }
    }
}
