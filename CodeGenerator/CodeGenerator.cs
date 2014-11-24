using SimpleLang.MiddleEnd;

namespace SimpleLang.CodeGenerator
{
    interface CodeGenerator
    {
        string code(ControlFlowGraph CFG);
    }
}