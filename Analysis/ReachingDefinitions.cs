using System;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    public class KillGenContext : Context<Tuple<BitSet,BitSet>>
    {
        public int DefsCount { get; private set; }

        public KillGenContext(ControlFlowGraph cfg)
            : base(cfg)
        {
            //Хотим узнать общее количество определений
            DefsCount = 0;
            //Вспомогательные структуры - словарь для номеров d, относящихся к соотв. блоку
            Dictionary<BaseBlock, HashSet<int>> BlockDef = new Dictionary<BaseBlock, HashSet<int>>();
            //Соответствие "номер d" - "имя определяемой переменной"
            List<string> DefVar = new List<string>();
            foreach(BaseBlock bl in Graph.GetBlocks())
            {
                BlockDef[bl] = new HashSet<int>();
                foreach (CodeLine cl in bl.Code)
                    if (cl.Operation == ":=")
                    {
                        BlockDef[bl].Add(DefsCount++);
                        DefVar.Add(cl.First);
                    }
            }
            //Для каждого базового блока легко можем сформировать элементы Gen
            foreach (BaseBlock bl in Graph.GetBlocks())
            {
                this[bl] = new Tuple<BitSet, BitSet>(new BitSet(DefsCount), new BitSet(DefsCount));
                foreach (int i in BlockDef[bl])
                    this[bl].Item2.Set(i, true);
            }
            //to be continued....
        }

        public BitSet Kills(BaseBlock bl)
        {
            return this[bl].Item1;
        }

        public BitSet Gens(BaseBlock bl)
        {
            return this[bl].Item2;
        }
    }

    public class ReachingDefsTransferFunction : TransferFunction<BitSet>
    {
        private Tuple<BitSet,BitSet> Info;

        public ReachingDefsTransferFunction(Tuple<BitSet,BitSet> info)
        {
            Info = info;
        }

        public BitSet Transfer(BitSet input)
        {
            return (BitSet)Info.Item1.Union(input.Subtract(Info.Item2));
        }
    }

    public class ReachingDefsAlgorithm : DownTopAlgorythm<Tuple<BitSet, BitSet>, BitSet>
    {
        private int DataSize;

        public ReachingDefsAlgorithm(ControlFlowGraph cfg)
        {
            In = new Dictionary<BaseBlock, BitSet>();
            Out = new Dictionary<BaseBlock, BitSet>();
            Cont = new KillGenContext(cfg);

            DataSize = ((KillGenContext)Cont).DefsCount;
            foreach (BaseBlock bl in cfg.GetBlocks())
            {
                In[bl] = new BitSet(DataSize);
                Out[bl] = new BitSet(DataSize);
                Func[bl] = new ReachingDefsTransferFunction(Cont[bl]);
            }
        }

        public override Tuple<Dictionary<BaseBlock, BitSet>, Dictionary<BaseBlock, BitSet>> Apply()
        {
            return base.Apply(new BitSet(DataSize),new BitSet(DataSize),BitSet.Union);
        }
    }
}