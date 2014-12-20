using System;
using System.Linq;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    //Множество, в котором элементы представлены флажками true/false
    public class BitSet : IndexedSet<int, bool>, ICloneable, IEquatable<BitSet>
    {
        private bool[] Elems;

        public BitSet(int size)
        {
            Elems = Enumerable.Repeat(false, size).ToArray();
        }

        public int Count { get { return Elems.Length; } }

        public BitSet(bool[] elems)
        {
            Elems = (bool[])elems.Clone();
        }

        public bool Get(int index)
        {
            return Elems[index];
        }

        public void Set(int index, bool value)
        {
            Elems[index] = value;
        }

        public object Clone()
        {
            return new BitSet(Elems);
        }

        public bool Equals(BitSet other)
        {
            if (other.Elems.Length != Elems.Length)
                return false;
            for (int i = 0; i < Elems.Length; ++i)
                if (Elems[i] != other.Elems[i])
                    return false;
            return true;
        }

        public override string ToString()
        {
            return System.String.Join(" ", Elems.Select(e => e.ToString()));
        }

        public static BitSet Intersect(BitSet a, BitSet b)
        {
            return (BitSet)a.Intersect(b);
        }

        public static BitSet Union(BitSet a, BitSet b)
        {
            return (BitSet)a.Union(b);
        }

        public static BitSet Subtract(BitSet a, BitSet b)
        {
            return (BitSet)a.Subtract(b);
        }

        public IndexedSet<int, bool> Intersect(IndexedSet<int, bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f && s).ToArray());
        }

        public IndexedSet<int, bool> Union(IndexedSet<int, bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f || s).ToArray());
        }

        public IndexedSet<int, bool> Subtract(IndexedSet<int, bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => s ? false : f).ToArray());
        }
    }

    public class KillGenContext : Context<Tuple<BitSet,BitSet>>
    {
        public int Count { get; protected set; }

        public KillGenContext(ControlFlowGraph cfg)
            : base(cfg)
        {
            //Хотим узнать общее количество определений
            Count = 0;
            //Вспомогательные структуры - словарь для номеров d, относящихся к соотв. блоку
            Dictionary<BaseBlock, HashSet<int>> BlockDef = new Dictionary<BaseBlock, HashSet<int>>();
            //Соответствие "номер d" - "имя определяемой переменной"
            Dictionary<string,HashSet<int>> VarDef = new Dictionary<string,HashSet<int>>();
            foreach(BaseBlock bl in Graph.GetBlocks())
            {
                BlockDef[bl] = new HashSet<int>();
                foreach (CodeLine cl in bl.Code)
                    if (cl.Operator == OperatorType.Assign && !cl.First.StartsWith("_t"))
                    {
                        if(!VarDef.ContainsKey(cl.First))
                            VarDef[cl.First] = new HashSet<int>();
                        VarDef[cl.First].Add(Count);
                        BlockDef[bl].Add(Count++);
                    }
            }
            //Для каждого базового блока легко можем сформировать элементы Gen и Kill
            foreach (BaseBlock bl in Graph.GetBlocks())
            {
                this[bl] = new Tuple<BitSet, BitSet>(new BitSet(Count), new BitSet(Count));
                foreach (int i in BlockDef[bl])
                {
                    this[bl].Item2.Set(i, true);
                    foreach(string name in VarDef.Keys)
                        if(VarDef[name].Contains(i))
                        {
                            foreach(int j in VarDef[name])
                                this[bl].Item1.Set(j,true);
                            this[bl].Item1.Set(i,false);
                        }        
                }
            }
        }

        public BitSet Kill(BaseBlock bl)
        {
            return this[bl].Item1;
        }

        public BitSet Gen(BaseBlock bl)
        {
            return this[bl].Item2;
        }
    }

    public class ReachingDefsTransferFunction : InfoProvidedTransferFunction<Tuple<BitSet,BitSet>,BitSet>
    {
        public ReachingDefsTransferFunction(Tuple<BitSet, BitSet> info)
            : base(info)
        { }

        public override BitSet Transfer(BitSet input)
        {
            return (BitSet)Info.Item2.Union(input.Subtract(Info.Item1));
        }
    }

    public class ReachingDefsAlgorithm : TopDownAlgorithm<Tuple<BitSet, BitSet>, KillGenContext, BitSet>
    {
        protected int DataSize;

        public ReachingDefsAlgorithm(ControlFlowGraph cfg)
            : base(cfg)
        {
            DataSize = Cont.Count;
            foreach (BaseBlock bl in cfg.GetBlocks())
            {
                In[bl] = new BitSet(DataSize);
                Out[bl] = new BitSet(DataSize);
                Func[bl] = new ReachingDefsTransferFunction(Cont[bl]);
            }
        }

        public override Tuple<Dictionary<BaseBlock, BitSet>, Dictionary<BaseBlock, BitSet>> Apply()
        {
            return base.Apply(new BitSet(DataSize), new BitSet(DataSize), new BitSet(DataSize), BitSet.Union);
        }
    }
}