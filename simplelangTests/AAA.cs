using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCompiler;
using SimpleLang.MiddleEnd;
using SimpleLang.Optimizations;

namespace simplelangTests
{
    [TestClass]
    public class AAA
    {
        [TestMethod]
        public void CleanDead_06()
        {
            BlockNode Root = SimpleCompilerMain.SyntaxAnalysis("../../_Texts/Test2.txt");
            if (Root != null && SimpleCompilerMain.SemanticAnalysis(Root))
            {
                var CFG = SimpleCompilerMain.BuildCFG(Root);
                SimpleCompilerMain.PrintCFG(CFG);
                CleanDead cl = new CleanDead();
                cl.Optimize(CFG.GetBlocks().First.Next.Value);
                SimpleCompilerMain.PrintCFG(CFG);
                Assert.AreEqual(6, CFG.GetBlocks().First.Next.Value.Code.Count);
            }
        }
    }
}