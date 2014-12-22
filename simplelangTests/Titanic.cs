using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleCompiler;
using SimpleLang.Analysis;
using System.Collections.Generic;

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
                SetAdapter<Expression> e = new SetAdapter<Expression>();
                Expression e2 = new Expression("m","1",BinOpType.Minus);
                e.Add(e2);
                Assert.IsTrue(e.Equals(AEAResult.Item2[CFG.BlockAt(1)]));
            }
        }
        [TestMethod]
        public void DominatorTree_25()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                DominatorsTree DTree = new DominatorsTree(CFG);
                Stack<DominatorsTree.TreeNode<BaseBlock>> Path =new Stack<DominatorsTree.TreeNode<BaseBlock>>();
                Dictionary<int,HashSet<int>> d = new  Dictionary<int,HashSet<int>>();
                Path.Push(DTree.Root);
                while (Path.Count > 0)
                {
                    var Top = Path.Pop();
                    if (Top.Items.Count > 0)
                    {
                        d[Top.Value.nBlock] = new HashSet<int>();
                        foreach (var it in Top.Items)
                        {
                            d[Top.Value.nBlock].Add(it.Value.nBlock);
                            Path.Push(it);
                        }
                    }
                }
                SimpleCompilerMain.RunDominatorTree(CFG);
                Assert.IsTrue(d[3].Contains(4) && d[3].Contains(5) && d[3].Contains(6));

            }
        }
    }
}
