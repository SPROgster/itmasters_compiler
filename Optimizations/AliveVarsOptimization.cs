using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    public class AliveVarsOptimization
    {
        public static void optimize(Tuple<Dictionary<BaseBlock, SetAdapter<string>>, Dictionary<BaseBlock, SetAdapter<string>>> ava, 
                                    ControlFlowGraph CFG)
        {
            // Если программа состоит из одного блока (+1 CFG.GetStart, +1 CFG.GetEnd)
            // То выходим, т.к. оптимизация межблочная
            if (ava.Item1.Keys.Count <= 3) 
                return;
            foreach (var block in ava.Item1.Keys)
                if (block != CFG.GetStart() && block != CFG.GetEnd())
                {
                    List<CodeLine> res = block.Code.ToList<CodeLine>(); // creating list to delete from
                    for (int i = 0; i < res.Count; ++i)
                    {
                        var codeline = res[i];
                        var left = codeline.First;
                        if ((codeline.Operator.ToString() != "Assign")) // If it's not assign - save the codeline
                            continue;
                        if (left.StartsWith("_t")) // skip temp variable
                            continue;
                        // Maybe there is not third variable in assign node
                        if ((!codeline.Second.Equals(left)) && ( (codeline.Third == null) || (!codeline.Third.Equals(left)) ))
                            if (!(ava.Item2[block].Contains(left))) // if left not contains in Out
                            {
                                res.RemoveAt(i); // deleting and moving iterator
                                --i;
                            }
                    }
                    block.Code = new LinkedList<CodeLine>(res); // back assign
                }
        }
    }
}
