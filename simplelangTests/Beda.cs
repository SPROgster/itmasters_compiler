using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;
using System;
using System.Linq;
using System.Collections.Generic;

namespace simplelangTests
{
    [TestClass]
    public class Beda
    {
        [TestMethod]
        public void DeadOrAlive_02()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test2.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SimpleCompilerMain.PrintCFG(CFG);
                List<CodeLine> bl = new List<CodeLine>(CFG.GetBlocks().First.Next.Value.Code);
                for (int i = 0; i < bl.Count; i++)
                {
                    Console.WriteLine(bl[i] + " " + DeadOrAlive.IsDead(CFG.GetBlocks().First.Next.Value, bl[i].First, i));
                }

                bool alive =
                    DeadOrAlive.IsAliveBeforeLine(CFG.GetBlocks().First.Next.Value, "i", 0) &&
                    DeadOrAlive.IsAliveAfterLine(CFG.GetBlocks().First.Next.Value, "i", 0);
                Assert.IsFalse(alive);
            }
        }

        [TestMethod]
        public void GenKill_11()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SimpleCompilerMain.PrintCFG(CFG);
                KillGenContext egc = new KillGenContext(CFG);
                foreach (var item in CFG.GetBlocks())
                {
                    Console.WriteLine(item);
                    Console.WriteLine(egc.Gen(item).ToString().Replace("True", "1").Replace("False", "0"));
                    Console.WriteLine(egc.Kill(item).ToString().Replace("True", "1").Replace("False", "0"));
                    Console.WriteLine();
                }
                BitSet resultgen = egc.Gen(CFG.GetBlocks().First.Next.Value);
                BitSet rightgen = new BitSet(new bool[] { true, true, true, false, false, false, false });
                Assert.IsTrue(resultgen.Equals(rightgen));
            }
        }

        [TestMethod]
        public void DeadOrAliveBBL_13()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test6.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                AliveVarsAlgorithm AVA = new AliveVarsAlgorithm(CFG);
                var AVAResult = AVA.Apply();
                foreach (var block in AVAResult.Item1.Keys)
                    if (block != CFG.GetStart() && block != CFG.GetEnd())
                    {
                        Console.WriteLine(block);
                        Console.WriteLine("In:\t" + AVAResult.Item1[block]);
                        Console.WriteLine("Out:\t" + AVAResult.Item2[block]);
                    }
            }
        }
        [TestMethod]
        public void NaturalCycles_29()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test3.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                NaturalCycles n = new NaturalCycles();
                var l=n.GetLoop(CFG, 6, 1).Select(x=>x.nBlock).OrderBy(x=>x).ToList();
                var rightl=new List<int>(new int[] {1,3,4,5,6});
                Assert.IsTrue(l.SequenceEqual(rightl));
            }
        }
    }
}