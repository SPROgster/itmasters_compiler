using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Analysis;
using SimpleLang.Visitors;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleLang.CodeGenerator;
using System.IO;
using SimpleScanner;
using SimpleParser;

namespace SimpleLangTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSymbolTable()
        {
            string FileName = "../../_Texts/Test1.txt";
            string Text = File.ReadAllText(FileName);
            Scanner scanner = new Scanner();
            scanner.SetSource(Text, 0);
            Parser parser = new Parser(scanner);
            var b = parser.Parse();
            var sne = new CheckVariablesVisitor();
            parser.root.Visit(sne);
            Assert.AreEqual(true,SymbolTable.Contains("i"), "В таблице символов не содержится переменная i");
            Assert.AreEqual(true,SymbolTable.Contains("k"), "В таблице символов не содержится переменная i");
        }
        [TestMethod]
        public void TestThreeCode()
        {
            string FileName = "../../_Texts/Test2.txt";
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
            Assert.AreEqual(true,gcv.Code.First.Value.First.Equals("i"));
        }
    }
}
