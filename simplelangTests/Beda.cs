using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;
using SimpleLang.Visitors;
using SimpleParser;
using SimpleScanner;
using System;
using System.Collections.Generic;
using System.IO;

namespace simplelangTests
{
    [TestClass]
    public class Beda
    {
        [TestMethod]
        public void DeadOrAlive_1_1()
        {
            string FileName = @"..\..\_Texts\Test2.txt";
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

        [TestMethod]
        public void GenKill_2_5()
        {
            string FileName = @"..\..\_Texts\Test3.txt";
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
        [TestMethod]
        public void DeadOrAliveBBL_3_0()
        {
            string FileName = @"..\..\_Texts\Test6.txt";
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
}