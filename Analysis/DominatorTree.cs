using System;
using System.Linq;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    using BlockSet = SetAdapter<BaseBlock>;

    public class DominatorTree
    {
        private class EmptyContext: Context<BaseBlock>
        {
            public EmptyContext(ControlFlowGraph cfg)
                :base(cfg)
            { }
        }

        private class DominatorsAlgorithm : TopDownAlgorithm<BaseBlock, EmptyContext, BlockSet>
        {
            public class DominatorsTransferFunction: InfoProvidedTransferFunction<BaseBlock, BlockSet>
            {
                public DominatorsTransferFunction(BaseBlock info)
                    : base(info)
                { }

                public override BlockSet Transfer(BlockSet input)
                {
                    var Result = (BlockSet)input.Clone();
                    Result.Add(Info);
                    return Result;
                }
            }

            public DominatorsAlgorithm(ControlFlowGraph cfg)
                : base(cfg)
            {
                foreach (BaseBlock bl in cfg.GetBlocks())
                {
                    In[bl] = new BlockSet();
                    Out[bl] = new BlockSet();
                    Func[bl] = new DominatorsTransferFunction(bl);
                }
            }

            public override Tuple<Dictionary<BaseBlock, BlockSet>, Dictionary<BaseBlock, BlockSet>> Apply()
            {
                BlockSet AllBlocks = new BlockSet();
                foreach (BaseBlock bl in this.Cont.Blocks)
                    AllBlocks.Add(bl);
                return base.Apply(new BlockSet(Cont.Start), AllBlocks, AllBlocks, BlockSet.Intersect);
            }
        }

        public DominatorTree(ControlFlowGraph cfg)
        {

        }
    }
}
