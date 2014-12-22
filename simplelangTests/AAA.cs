using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleParser;
using SimpleScanner;
using System;
using System.Collections.Generic;
using System.IO;

namespace simplelangTests
{
    [TestClass]
    public class AAA
    {
        [TestMethod]
        public void CleanDead_06()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/CleanDead_06.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SimpleCompilerMain.PrintCFG(CFG,true);
                CleanDead cl = new CleanDead();
                cl.Optimize(CFG.BlockAt(1));
                SimpleCompilerMain.PrintCFG(CFG,true);
                Assert.AreEqual(5, CFG.BlockAt(1).Code.Count);
            }
        }
        [TestMethod]
        public void FormulaTF_2_4()
        {
             BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test2.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);

                 KillGenContext cont = new KillGenContext(CFG);
                 List<Tuple<BitSet, BitSet>> tupl = new List<Tuple<BitSet, BitSet>>();
                 foreach (var line in CFG.GetBlocks().First.List)
                 {
                     tupl.Add(new Tuple<BitSet, BitSet>(cont.Gen(line), cont.Kill(line)));
                 }
                 FormulaTransferFunction tf = new FormulaTransferFunction(tupl);
                 BitSet input = new BitSet(cont.Count);
                 BitSet output = tf.Transfer(input);
                 BitSet success = new BitSet(new bool[] { true, false, true, false, false });

                 Assert.IsTrue(output.Equals(success));
             }
        }
    }
}