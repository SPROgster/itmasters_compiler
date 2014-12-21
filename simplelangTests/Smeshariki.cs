using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;
using SimpleLang.Visitors;
using SimpleParser;
using SimpleScanner;
using System.IO;

namespace simplelangTests
{
    [TestClass]
    public class Smeshariki
    {
        [TestMethod]
        public void CSE_04()
        {
             BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test4.txt");
             if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
             {
                 var CFG = SimpleCompilerMain.BuildCFG(Root);
                 CSE cse = new CSE();
                 SimpleCompilerMain.PrintCFG(CFG);
                 cse.Optimize(CFG.GetBlocks().First.Next.Value);
                 SimpleCompilerMain.PrintCFG(CFG);
                 string f = CFG.GetBlocks().First.Next.Value.Code.First.Value.First;
                 Assert.IsTrue(f.StartsWith("_t"));
                 Assert.IsTrue(CFG.GetBlocks().First.Next.Value.Code.First.Value.Second.Equals("b"));
                 Assert.IsTrue(CFG.GetBlocks().First.Next.Value.Code.First.Value.Third.Equals("c"));
             }
        }
    }
}