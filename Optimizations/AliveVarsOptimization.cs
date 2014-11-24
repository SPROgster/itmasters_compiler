using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    class AliveVarsOptimization
    {
        public static void optimize(Tuple<Dictionary<BaseBlock, SetAdapter<string>>, Dictionary<BaseBlock, SetAdapter<string>>> ava, 
                                    ControlFlowGraph CFG)
        {
            foreach (var block in ava.Item1.Keys)
                if (block != CFG.GetStart() && block != CFG.GetEnd())
                {
                    for (var codeline_iter = block.Code.First; codeline_iter != null; codeline_iter = codeline_iter.Next)
                    {
                        var codeline = codeline_iter.Value;
                        var left = codeline.First;
                        if ((codeline.Second != left) && (codeline.Third != left))
                        {
                            if (ava.Item2[block].ToString().IndexOf(left) == -1)
                            {
                                block.Code.Remove(codeline_iter.Value);
                            }
                        }
                    }
                }
        }
    }
}
