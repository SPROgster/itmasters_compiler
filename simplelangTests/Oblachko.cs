using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using System.Collections.Generic;
using SimpleCompiler;

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
            Assert.AreEqual("5",myList[3].Second);
            Assert.AreEqual("-2", myList[4].Second);
            Assert.AreEqual("4", myList[5].Second);
        }
    }
}
