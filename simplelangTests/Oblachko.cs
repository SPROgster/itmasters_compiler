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
    public class Oblachko
    {
        [TestMethod]
        public void Fold_03()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test5.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SimpleCompilerMain.PrintCFG(CFG);
                Fold.fold(ref CFG.GetBlocks().First.Value.Code);
                SimpleCompilerMain.PrintCFG(CFG);
                //Assert.AreEqual("5", myList[3].Second);
                //Assert.AreEqual("-2", myList[4].Second);
                //Assert.AreEqual("4", myList[5].Second);
            }
        }
        [TestMethod]
        public void RDA_12()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                Console.WriteLine();
                ReachingDefsAlgorithm RDA = new ReachingDefsAlgorithm(CFG);
                var RDAResult = RDA.Apply();
                foreach (var block in RDAResult.Item1.Keys)
                    if (block != CFG.GetStart() && block != CFG.GetEnd())
                    {
                        Console.WriteLine(block);
                        Console.WriteLine("In:\t" + RDAResult.Item1[block].ToString().Replace("True", "1").Replace("False", "0"));
                        Console.WriteLine("Out:\t" + RDAResult.Item2[block].ToString().Replace("True", "1").Replace("False", "0"));
                    }
                List<BaseBlock> bl = new List<BaseBlock>(CFG.GetBlocks());
                BitSet resultin = RDAResult.Item1[bl[3]];
                BitSet rightin = new BitSet(new bool[] { true, true, true, false, true, true, true });
                Assert.IsTrue(resultin.Equals(rightin));
            }
        }
        [TestMethod]
        public void AliveVars_14()
        {
             BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);
                 Console.WriteLine();
                 SimpleCompilerMain.PrintCFG(CFG);
                 AliveVarsOptimization a = new AliveVarsOptimization();
                 a.Optimize( CFG);
                 SimpleCompilerMain.PrintCFG(CFG);
                 List<BaseBlock> l = new List<BaseBlock>(CFG.GetBlocks());
                 Assert.AreEqual(2, l[1].Code.Count);//удалило переменную a в первом блоке
                 Assert.AreEqual(0, l[6].Code.Count);//удалило переменную a в блоке if
             }
        }
    }
}