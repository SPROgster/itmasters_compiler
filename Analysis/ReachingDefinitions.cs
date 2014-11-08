using System;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    public class KillGenInfo
    {
        public BitSet Kill { get; private set; }
        public BitSet Gen { get; private set; }

        public KillGenInfo(int size)
        {
            Kill = new BitSet(size);
            Gen = new BitSet(size);
        }
    }

    public class KillGenContext : Context<KillGenInfo>
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
            Dictionary<string,HashSet<int>> VarDef = new Dictionary<string,HashSet<int>>();
            foreach(BaseBlock bl in Graph.GetBlocks())
            {
                BlockDef[bl] = new HashSet<int>();
                foreach (CodeLine cl in bl.Code)
                    if (cl.Operation != "i" && cl.Operation != "g" && cl.Operation != "nop")
                    {
                        if(!VarDef.ContainsKey(cl.First))
                            VarDef[cl.First] = new HashSet<int>();
                        VarDef[cl.First].Add(DefsCount);
                        BlockDef[bl].Add(DefsCount++);
                    }
            }
            //Для каждого базового блока легко можем сформировать элементы Gen и Kill
            foreach (BaseBlock bl in Graph.GetBlocks())
            {
                this[bl] = new KillGenInfo(DefsCount);
                foreach (int i in BlockDef[bl])
                {
                    this[bl].Gen.Set(i, true);
                    foreach(string name in VarDef.Keys)
                        if(VarDef[name].Contains(i))
                        {
                            foreach(int j in VarDef[name])
                                this[bl].Kill.Set(j,true);
                            this[bl].Kill.Set(i,false);
                        }        
                }
            }
        }

        public BitSet Kills(BaseBlock bl)
        {
            return this[bl].Kill;
        }

        public BitSet Gens(BaseBlock bl)
        {
            return this[bl].Gen;
        }
    }

    public class ReachingDefsTransferFunction : TransferFunction<BitSet>
    {
        private KillGenInfo Info;

        public ReachingDefsTransferFunction(KillGenInfo info)
        {
            Info = info;
        }

        public BitSet Transfer(BitSet input)
        {
            return (BitSet)Info.Gen.Union(input.Subtract(Info.Kill));
        }
    }

    public class ReachingDefsAlgorithm : DownTopAlgorythm<KillGenInfo, BitSet>
    {
        private int DataSize;

        public ReachingDefsAlgorithm(ControlFlowGraph cfg)
        {
            In = new Dictionary<BaseBlock, BitSet>();
            Out = new Dictionary<BaseBlock, BitSet>();
            Func = new Dictionary<BaseBlock, TransferFunction<BitSet>>();
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
            return base.Apply(new BitSet(DataSize),new BitSet(DataSize),new BitSet(DataSize),BitSet.Union);
        }
    }
}