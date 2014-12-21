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
    public class Smeshariki
    {
        [TestMethod]
        public void CSE_04()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/CSE_04.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);
                 CSE cse = new CSE();
                 SimpleCompilerMain.PrintCFG(CFG,true);
                 cse.Optimize(CFG.GetBlocks().First.Next.Value);
                 SimpleCompilerMain.PrintCFG(CFG,true);
                 string f = CFG.GetBlocks().First.Next.Value.Code.First.Value.First;
                 Assert.IsTrue(f.StartsWith("_t"));
                 Assert.IsTrue(CFG.GetBlocks().First.Next.Value.Code.First.Value.Second.Equals("b"));
                 Assert.IsTrue(CFG.GetBlocks().First.Next.Value.Code.First.Value.Third.Equals("c"));
             }
        }

        [TestMethod]
        public void SpanningTree_26()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SpanningTree spTree = new SpanningTree(CFG);
                List<int> l=new List<int>();
                foreach (BaseBlock block in spTree.spTree)
                {
                    l.Add(block.nBlock);
                }
                Assert.IsTrue(l[3] == 7);//проверяем перенумеровали или нет
            }
        }
    }
}