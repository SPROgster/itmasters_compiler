using System;
using System.Linq;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    using BlockSet = SetAdapter<BaseBlock>;

    public class DominatorsTree
    {
        public TreeNode<BaseBlock> Root;

        public DominatorsTree(ControlFlowGraph cfg)
        {
            var Dominators = (new DominatorsAlgorithm(cfg)).Apply().Item2;
            var DirectDominators = new Dictionary<BaseBlock, TreeNode<BaseBlock>>();
            //Находим непосредственных доминаторов после того, как уже нашли всех
            foreach (BaseBlock block in Dominators.Keys)
                DirectDominators[block] = new TreeNode<BaseBlock>(block);
            foreach (BaseBlock block in Dominators.Keys)
                foreach (BaseBlock dblock in
                    Dominators.Where(e => e.Key != block && Dominators[block].Contains(e.Key)).Select(e => e.Value).
                    Aggregate(new BlockSet(), (a, b) => BlockSet.Union(BlockSet.Subtract(a, b), BlockSet.Subtract(b, a))))
                    DirectDominators[dblock].AddItem(DirectDominators[block]);
            Root = DirectDominators[cfg.GetStart()];
        }

        /******************************************************************************/

        public class TreeNode<T>
        {
            public T Value;
            public LinkedList<TreeNode<T>> Items;

            public TreeNode(T val)
            {
                Value = val;
                Items = new LinkedList<TreeNode<T>>();
            }

            public void AddItem(TreeNode<T> i)
            {
                Items.AddLast(i);
            }
        }

        private class EmptyContext: Context<BaseBlock>
        {
            public EmptyContext(ControlFlowGraph cfg)
                :base(cfg)
            { }
        }

        private class DominatorsAlgorithm : TopDownAlgorithm<BaseBlock, EmptyContext, BlockSet>
        {
            public class DominatorsTransferFunction : InfoProvidedTransferFunction<BaseBlock, BlockSet>
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
    }
}
