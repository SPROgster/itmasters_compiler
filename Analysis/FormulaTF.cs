using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleLang.Analysis
{
    public class FormulaTransferFunction : InfoProvidedTransferFunction<List<Tuple<BitSet, BitSet>>, BitSet>
    {
        public FormulaTransferFunction(List<Tuple<BitSet, BitSet>> info)
            : base(info)
        { }

        public override BitSet Transfer(BitSet input)
        {
            BitSet result = (BitSet) input.Clone();
            foreach (var a in Info)
            {
                Console.WriteLine(result.ToString());
                result = (BitSet)a.Item2.Union(result.Subtract(a.Item1));
            }
            return result;
        }
    }

}
