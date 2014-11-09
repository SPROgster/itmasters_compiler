using System;
using System.Linq;
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

    public class StringSet : ChaoticSet<string>, ICloneable
    {
        private HashSet<string> Elems;

        public StringSet()
        {
            Elems = new HashSet<string>();
        }

        private StringSet(string[] elems)
        {
            Elems = new HashSet<string>(elems);
        }

        public void Add(string elem)
        {
            Elems.Add(elem);
        }

        public void Remove(string elem)
        {
            Elems.Remove(elem);
        }

        public bool Contains(string elem)
        {
            return Elems.Contains(elem);
        }

        public ISet<string> Intersect(ISet<string> b)
        {
            return new StringSet(Elems.Intersect(((StringSet)b).Elems).ToArray());
        }

        public ISet<string> Union(ISet<string> b)
        {
            return new StringSet(Elems.Union(((StringSet)b).Elems).ToArray());
        }

        public ISet<string> Subtract(ISet<string> b)
        {
            return new StringSet(Elems.Except(((StringSet)b).Elems).ToArray());
        }

        public int Count
        {
            get { return Elems.Count; }
        }

        public object Clone()
        {
            return new StringSet(Elems.ToArray());
        }

        public override bool Equals(object obj)
        {
            if (obj is StringSet)
            {
                StringSet Second = (StringSet)obj;
                if (Second.Elems.Count != Elems.Count)
                    return false;
                foreach(string s in Elems)
                    if(!Second.Elems.Contains(s))
                        return false;
                return true;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Elems.GetHashCode();
        }

        public override string ToString()
        {
            return System.String.Join(" ", Elems.Select(e => e.ToString()));
        }

        public static StringSet Intersect(StringSet a, StringSet b)
        {
            return (StringSet)a.Intersect(b);
        }

        public static StringSet Union(StringSet a, StringSet b)
        {
            return (StringSet)a.Union(b);
        }

        public static StringSet Subtract(StringSet a, StringSet b)
        {
            return (StringSet)a.Subtract(b);
        }
    }

    public class DefUseContext : Context<Tuple<StringSet, StringSet>>
    {
        private bool GoodOperand(BaseBlock block, string s)
        {
            return s != null && (Char.IsLetter(s[0]) || s[0]=='_') && !this[block].Item1.Contains(s);
        }

        public DefUseContext(ControlFlowGraph cfg)
            : base(cfg)
        {
            foreach (BaseBlock block in cfg.GetBlocks())
            {
                this[block] = new Tuple<StringSet, StringSet>(new StringSet(), new StringSet());
                foreach (CodeLine line in block.Code)
                {
                    //Дурацкая проверка операции
                    //Надо учитывать использование в условии перехода?
                    //У нас в грамматике странно обстоят дела с типом bool...
                    if (line.First != null && line.Operation != "g" && line.Operation != "i")
                    {
                        if (GoodOperand(block,line.Second))
                            this[block].Item2.Add(line.Second);
                        if (GoodOperand(block,line.Third))
                            this[block].Item2.Add(line.Third);
                        this[block].Item1.Add(line.First);
                    }
                }
            }
        }

        public StringSet Def(BaseBlock bl)
        {
            return this[bl].Item1;
        }

        public StringSet Use(BaseBlock bl)
        {
            return this[bl].Item2;
        }
    }

    public class AliveVarsAlgorithm : TopDownAlgorithm<Tuple<StringSet, StringSet>, DefUseContext, StringSet>
    {
        public AliveVarsAlgorithm(ControlFlowGraph cfg):base(cfg)
        {
            foreach (BaseBlock bl in cfg.GetBlocks())
            {
                In[bl] = new StringSet();
                Out[bl] = new StringSet();
                Func[bl] = new AliveVarsTransferFunction(Cont[bl]);
            }
        }

        public override Tuple<Dictionary<BaseBlock, StringSet>, Dictionary<BaseBlock, StringSet>> Apply()
        {
            return base.Apply(new StringSet(), new StringSet(), new StringSet(), StringSet.Union);
        }
    }

    public class AliveVarsTransferFunction : InfoProvidedTransferFunction<Tuple<StringSet, StringSet>, StringSet>
    {
        public AliveVarsTransferFunction(Tuple<StringSet, StringSet> info)
            : base(info)
        { }

        public override StringSet Transfer(StringSet input)
        {
            return (StringSet)Info.Item2.Union(input.Subtract(Info.Item1));
        }
    }
}