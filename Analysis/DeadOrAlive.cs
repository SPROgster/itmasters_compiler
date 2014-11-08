using System;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{

    internal class DeadOrAlive
    {
        public static List<Tuple<string, string, int>> GenerateDefUse(List<CodeLine> l)
        {
            List<Tuple<string, string, int>> l2 = new List<Tuple<string, string, int>>();
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].First != null && SymbolTable.Contains(l[i].First))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].First, "def", i));
                }
                if (l[i].Second != null && SymbolTable.Contains(l[i].Second))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].Second, "use", i));
                }
                if (l[i].Third != null && SymbolTable.Contains(l[i].Third))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].Third, "use", i));
                }
            }
            return l2;
        }

        public static bool IsAlive(BaseBlock bl, string id, int line)
        {
            List<CodeLine> bl2 = new List<CodeLine>(bl.Code);
            bool alive;
            if (id.Length >= 2 && id.Substring(0, 2).Equals("_t"))
            {
                alive = false;
            }
            else
            {
                alive = true;
            }

            for (int i = bl2.Count - 1; i >= 0; i--)
            {
                if (bl2[i].Third != null && bl2[i].Third.Equals(id))
                {
                    alive = true;
                }
                if (bl2[i].Second != null && bl2[i].Second.Equals(id))
                {
                    alive = true;
                }                
                if (bl2[i].First != null && bl2[i].First.Equals(id))
                {
                    alive = false;
                }
                if (i == line)
                {
                    return alive;
                }
            }
            return alive;
        }
    }

    public class AliveVarsContext : Context<Tuple<BitSet, BitSet>>
    {
        public int Count { get; private set; }

        public AliveVarsContext(ControlFlowGraph cfg)
            : base(cfg)
        { }
    }

    //Уж очень похоже на ReachingDefinitions... Не обобщить ли?
    public class AliveVarsAlgorithm : TopDownAlgorithm<Tuple<BitSet, BitSet>, BitSet>
    {
        private int DataSize;

        public AliveVarsAlgorithm(ControlFlowGraph cfg): base(cfg)
        {
            Cont = new AliveVarsContext(cfg);
            DataSize = ((AliveVarsContext)Cont).Count;
            foreach (BaseBlock bl in cfg.GetBlocks())
            {
                In[bl] = new BitSet(DataSize);
                Out[bl] = new BitSet(DataSize);
                Func[bl] = new AliveVarsTransferFunction(Cont[bl]);
            }
        }

        public override Tuple<Dictionary<BaseBlock, BitSet>, Dictionary<BaseBlock, BitSet>> Apply()
        {
            return base.Apply(new BitSet(DataSize), new BitSet(DataSize), new BitSet(DataSize), BitSet.Union);
        }
    }

    //Уж очень она похожа на функцию для ReachingDefinitions...
    public class AliveVarsTransferFunction : TransferFunction<BitSet>
    {
        private Tuple<BitSet,BitSet> Info;

        public AliveVarsTransferFunction(Tuple<BitSet,BitSet> info)
        {
            Info = info;
        }

        public BitSet Transfer(BitSet input)
        {
            return (BitSet)Info.Item2.Union(input.Subtract(Info.Item1));
        }
    }
}