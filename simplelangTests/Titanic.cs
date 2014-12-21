using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleCompiler;
using SimpleLang.Analysis;

namespace simplelangTests
{
    [TestClass]
    public class Titanic
    {
        [TestMethod]
        public void GraphBBL_01()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/GraphBBL_01.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);
                 Assert.AreEqual(10, CFG.GetBlocks().Count);
             }
        }

        [TestMethod]
        public void AvailableExprs_15()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/AvailableExprs_15.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                AvailableExprsAlgorithm AEA = new AvailableExprsAlgorithm(CFG);
                var AEAResult = AEA.Apply();
                foreach (var block in AEAResult.Item1.Keys)
                    if (block != CFG.GetStart() && block != CFG.GetEnd())
                    {
                        Console.WriteLine(block);
                        Console.WriteLine("In:\t" + AEAResult.Item1[block]);
                        Console.WriteLine("Out:\t" + AEAResult.Item2[block]);
                    }
                Assert.AreEqual(1,AEAResult.Item2[CFG.BlockAt(1)].Count);
            }
        }
    }
}
