using System;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    public class KillGenContext : Context<Tuple<BitSet,BitSet>>
    {
        public KillGenContext(ControlFlowGraph cfg, BaseBlock block)
            : base(cfg, block)
        {
            //BitSet Gen, Kill;
        }
    }
}