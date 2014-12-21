using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleCompiler;

namespace simplelangTests
{
    [TestClass]
    public class Titanic
    {
        [TestMethod]
        public void GraphBBL_01()
        {
             BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);
                 Assert.AreEqual(10, CFG.GetBlocks().Count);
             }
        }
    }
}
