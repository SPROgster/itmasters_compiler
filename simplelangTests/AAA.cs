using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleLang.Visitors;
using SimpleScanner;
using SimpleParser;
using System.IO;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleCompiler;
using SimpleLang.Analysis;
using System.Collections.Generic;

namespace simplelangTests
{
    [TestClass]
    public class AAA
    {
        [TestMethod]
        public void CleanDead_1_5()
        {
            string FileName = @"..\..\Test2.txt";
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
            CleanDead cl = new CleanDead();
            cl.Optimize(CFG.GetBlocks().First.Next.Value);
            SimpleCompilerMain.PrintCFG(CFG);
            Assert.AreEqual(6, CFG.GetBlocks().First.Next.Value.Code.Count);
        }

        [TestMethod]
        public void FormulaTF_2_4()
        {
            string FileName = @"..\..\Test2.txt";
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
