using System;
using System.Collections.Generic;
using SimpleLang.MiddleEnd;
using SimpleLang.Analysis;

namespace SimpleLang.Optimizations
{
    public class CleanDead : LocalOptimization
    {
        public bool Optimize(BaseBlock block)
        {
            var list = DeadOrAlive.GenerateDefUse(new List<CodeLine>(block.Code));
            var result = true;

            for (var p = block.Code.First; p != null; p = p.Next)
            {
                foreach (Tuple<string, string, int> def in list)
                {
                    if (!DeadOrAlive.IsAlive(block, def.Item1, def.Item3))
                    {
                        block.Code.Remove(p);
                        result = false;
                    }
                }
            }

            return result;
        }
    }
}
