using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLang
{

    public interface IndexedSet<T>
    {
        T Get(int index);
        void Set(int index, T value);

        IndexedSet<T> Intersect(IndexedSet<T> b);
        IndexedSet<T> Union(IndexedSet<T> b);
        IndexedSet<T> Subtract(IndexedSet<T> b);
    }

    public class BitSet : IndexedSet<bool>, ICloneable
    {
        private bool[] Elems;

        public BitSet(int size)
        {
            Elems = Enumerable.Repeat(false, size).ToArray();
        }

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

        public IndexedSet<bool> Intersect(IndexedSet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f && s).ToArray());
        }

        public IndexedSet<bool> Union(IndexedSet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f, s) => f || s).ToArray());
        }

        public IndexedSet<bool> Subtract(IndexedSet<bool> b)
        {
            return new BitSet(Elems.Zip(((BitSet)b).Elems, (f,s) => s ? false : f).ToArray());
        }
    
        public object Clone()
        {
            return new BitSet(Elems);
        }
}

    public interface TransferFunction<T>
    {
        T Transfer(T input);
    }

    //public class MyCoolTransferFunctionWithBitSet:TransferFunction<BitSet>
    //{}

}
