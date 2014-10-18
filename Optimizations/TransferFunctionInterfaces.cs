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
    }

    public class BitSet : IndexedSet<bool>
    {
        private bool[] Elems;

        public BitSet(int size)
        {
            Elems = Enumerable.Repeat(false, size).ToArray();
        }

        public bool Get(int index)
        {
            return Elems[index];
        }

        public void Set(int index, bool value)
        {
            Elems[index] = value;
        }
    }

    public interface TransferFunction<T>
    {
        T Transfer(T input);
    }

    interface SetTransferFunction<S>:TransferFunction<IndexedSet<S>>
    {}

    //public class ConcreteTransferFunctionWithBitSet:SetTransferFunction<BitSet>
    //{}

}
