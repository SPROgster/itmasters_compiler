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
    public interface IndexedSet<IndexType, ValueType> : ISet<ValueType>
    {
        ValueType Get(IndexType index);
        void Set(IndexType index, ValueType value);
    }

    //Множество с доступом по индексу
    public interface ChaoticSet<ValueType> : ISet<ValueType>
    {
        void Add(ValueType elem);
        void Remove(ValueType elem);
        bool Contains(ValueType elem);
    }

    //Адаптация HashSet к использованию в алгоритмах
    public class SetAdapter<T> : HashSet<T>, IEquatable<SetAdapter<T>>, ICloneable
    {
        public SetAdapter(SetAdapter<T> elems)
            : base(elems)
        { }

        public SetAdapter(params T[] elems)
            : base()
        {
            for (int i = 0; i < elems.Length; ++i)
                this.Add(elems[i]);
        }

        public SetAdapter()
            : base()
        { }

        public object Clone()
        {
            return new SetAdapter<T>(this);
        }

        public bool Equals(SetAdapter<T> other)
        {
            if (other.Count != Count)
                return false;
            foreach (T e in this)
                if (!other.Contains(e))
                    return false;
            return true;
        }

        public override string ToString()
        {
            return System.String.Join(" ", this.Select(e => e.ToString()));
        }

        public static SetAdapter<T> Intersect(SetAdapter<T> a, SetAdapter<T> b)
        {
            return new SetAdapter<T>(a.Intersect(b).ToArray());
        }

        public static SetAdapter<T> Union(SetAdapter<T> a, SetAdapter<T> b)
        {
            return new SetAdapter<T>(a.Union(b).ToArray());
        }

        public static SetAdapter<T> Subtract(SetAdapter<T> a, SetAdapter<T> b)
        {
            return new SetAdapter<T>(a.Except(b).ToArray());
        }
    }

    //Интерфейс передаточной функции
    public interface TransferFunction<T>
    {
        T Transfer(T input);
    }

    public abstract class InfoProvidedTransferFunction<InfoType, DataType> : TransferFunction<DataType>
    {
        protected InfoType Info;

        public InfoProvidedTransferFunction(InfoType info)
        {
            Info = info;
        }

        public abstract DataType Transfer(DataType input);
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

    public abstract class Algorithm<InfoType, ContextType, DataType>
        where InfoType : class
        where ContextType: Context<InfoType>
    {
        protected delegate DataType CollectionFunction(DataType a, DataType b);

        //Контекст, специфичный для алгоритма и связанный с базовыми блоками
        protected ContextType Cont {get; private set;}
        //Множества, которые хотим построить
        protected Dictionary<BaseBlock, DataType> In {get; private set;}
        protected Dictionary<BaseBlock, DataType>  Out { get; private set; }
        //Передаточные функции для блоков
        protected Dictionary<BaseBlock, TransferFunction<DataType>> Func { get; private set; }

        protected abstract Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply(DataType endInit,
            DataType otherInit, DataType defaultInit, CollectionFunction collect);

        public abstract Tuple<Dictionary<BaseBlock, DataType>, Dictionary<BaseBlock, DataType>> Apply();

        protected Algorithm(ControlFlowGraph cfg)
        {
            In = new Dictionary<BaseBlock, DataType>();
            Out = new Dictionary<BaseBlock, DataType>();
            Cont = (ContextType)typeof(ContextType).
                GetConstructor(new Type[1] { typeof(ControlFlowGraph) }).
                Invoke(new object[1]{cfg});
            Func = new Dictionary<BaseBlock, TransferFunction<DataType>>();
        }
    }

    public abstract class DownTopAlgorithm<InfoType, ContextType, DataType> : Algorithm<InfoType, ContextType, DataType>
        where InfoType : class
        where ContextType : Context<InfoType>
        where DataType : IEquatable<DataType>
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

        protected DownTopAlgorithm(ControlFlowGraph cfg)
            : base(cfg)
        {}
    }

    public abstract class TopDownAlgorithm<InfoType, ContextType, DataType> : Algorithm<InfoType, ContextType, DataType>
        where InfoType : class
        where ContextType : Context<InfoType>
        where DataType : IEquatable<DataType>
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

        protected TopDownAlgorithm(ControlFlowGraph cfg)
            : base(cfg)
        {}
    }
}
