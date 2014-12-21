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
    }
}