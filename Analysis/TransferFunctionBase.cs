using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    //Множество
    public interface ISet<T>
    {
        ISet<T> Intersect(ISet<T> b);
        ISet<T> Union(ISet<T> b);
        ISet<T> Subtract(ISet<T> b);

        int Count { get; }
    }

    //Множество с доступом по индексу
    public interface IndexedSet<T> : ISet<T>
    {
        T Get(int index);
        void Set(int index, T value);
    }

    //Множество, в котором элементы представлены флажками true/false
    public class BitSet : IndexedSet<bool>, ICloneable
    {
        private bool[] Elems;

        public BitSet(int size)
        {
            Elems = Enumerable.Repeat(false, size).ToArray();
        }

        public int Count { get { return Elems.Length; } }

        private BitSet(bool[] elems)
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

        public ISet<bool> Intersect(ISet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f && s).ToArray());
        }

        public ISet<bool> Union(ISet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f || s).ToArray());
        }

        public ISet<bool> Subtract(ISet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f,s) => s ? false : f).ToArray());
        }
    
        public object Clone()
        {
            return new BitSet(Elems);
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet)
            {
                BitSet Second = (BitSet)obj;
                if (Second.Elems.Length != Elems.Length)
                    return false;
                for (int i = 0; i < Elems.Length; ++i)
                    if (Elems[i] != Second.Elems[i])
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
            return System.String.Join(" ",Elems.Select(e=>e.ToString()));
        }

        public static BitSet Intersect(BitSet a, BitSet b)
        {
            return (BitSet) a.Intersect(b);
        }

        public static BitSet Union(BitSet a, BitSet b)
        {
            return (BitSet) a.Union(b);
        }

        public static BitSet Subtract(BitSet a, BitSet b)
        {
            return (BitSet) a.Subtract(b);
        }
    }

    //Интерфейс передаточной функции
    public interface TransferFunction<T>
    {
        T Transfer(T input);
    }

    //Контекст, навешиваемый на граф базовых блоков
    public abstract class Context<T> where T: class
    {
        protected ControlFlowGraph Graph;
        private Dictionary<BaseBlock, T> Info;

        public Context(ControlFlowGraph cfg)
        {
            Graph = cfg;
            Info = new Dictionary<BaseBlock, T>();
            foreach (BaseBlock bl in cfg.GetBlocks())
                Info.Add(bl, null);
        }

        public T this[BaseBlock i]
        {
            get { return Info[i]; }
            set { Info[i] = value; }
        }

        //Возвращает список блоков, предшествующих заданному
        public LinkedList<BaseBlock> Inputs(BaseBlock bl)
        {
            return Graph.GetInputs(bl);
        }

        //Возвращает список блоков-потомков заданного
        public LinkedList<BaseBlock> Outputs(BaseBlock bl)
        {
            return Graph.GetOutputs(bl);
        }

        public BaseBlock Start { get { return Graph.GetStart(); } }
        public BaseBlock End { get { return Graph.GetEnd(); } }
        public LinkedList<BaseBlock> Blocks { get { return Graph.GetBlocks(); } }
    }

    public abstract class Algorithm<InfoType, DataType>
        where InfoType : class
    {
        protected delegate DataType CollectionFunction(DataType a, DataType b);

        //Контекст, специфичный для алгоритма и связанный с базовыми блоками
        protected Context<InfoType> Cont;
        //Множества, которые хотим построить
        protected Dictionary<BaseBlock, DataType> In, Out;
        //Передаточные функции для блоков
        protected Dictionary<BaseBlock, TransferFunction<DataType>> Func;

        protected abstract Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply(DataType endInit,
            DataType otherInit, DataType defaultInit, CollectionFunction collect);

        public abstract Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply();
    }

    public abstract class TopDownAlgorythm<InfoType, DataType> : Algorithm<InfoType, DataType>
        where InfoType : class
    {
        protected override Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply(DataType endInit,
            DataType otherInit, DataType defaultInit, CollectionFunction collect)
        {
            In[Cont.End] = endInit;
            foreach (BaseBlock block in Cont.Blocks)
                if (block != Cont.End)
                    In[block] = otherInit;
            bool SomethingChanged = true;
            while (SomethingChanged)
            {
                SomethingChanged = false;
                foreach (BaseBlock block in Cont.Blocks)
                    if (block != Cont.End)
                    {
                        Out[block] = Cont.Outputs(block).Select(bl => In[bl]).
                            Aggregate(defaultInit,(a, b) => collect(a, b));
                        if (SomethingChanged)
                            In[block] = Func[block].Transfer(Out[block]);
                        else
                        {
                            var NewIn = Func[block].Transfer(Out[block]);
                            if (!NewIn.Equals(In[block]))
                                SomethingChanged = true;
                            In[block] = NewIn;
                        }
                    }
            }
            return new Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>>(In, Out);
        }
    }

    public abstract class DownTopAlgorythm<InfoType, DataType> : Algorithm<InfoType, DataType>
        where InfoType : class
    {
        protected override Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply(DataType endInit, 
            DataType otherInit, DataType defaultInit, CollectionFunction collect)
        {
            Out[Cont.Start] = endInit;
            foreach(BaseBlock block in Cont.Blocks)
                if(block!=Cont.Start)
                    Out[block] = otherInit;
            bool SomethingChanged = true;
            while (SomethingChanged)
            {
                SomethingChanged = false;
                foreach(BaseBlock block in Cont.Blocks)
                    if(block!=Cont.Start)
                    {
                        In[block] = Cont.Inputs(block).Select(bl=>Out[bl]).
                            Aggregate(defaultInit,(a,b)=>collect(a,b));
                        if (SomethingChanged)
                            Out[block] = Func[block].Transfer(In[block]);
                        else
                        {
                            var NewOut = Func[block].Transfer(In[block]);
                            if (!NewOut.Equals(Out[block]))
                                SomethingChanged = true;
                            Out[block] = NewOut;
                        }
                    }
            }
            return new Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>>(In, Out);
        }
    }
}
