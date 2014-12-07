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
            var result = false;
            List<CodeLine> bl2 = new List<CodeLine>(block.Code);
            List<int> toDelete = new List<int>();
            for (int i = 0; i < bl2.Count; i++)
			{
                bool DeadBefore=!DeadOrAlive.IsAliveBeforeLine(block, bl2[i].First, i) ;
                bool DeadAfter=!DeadOrAlive.IsAliveAfterLine(block, bl2[i].First, i);
                if (DeadBefore && DeadAfter)
                {
                    toDelete.Add(i);
                    result = true;
                }
			}
            toDelete.Reverse();
            foreach (var item in toDelete)
            {
                bl2.RemoveAt(item);
            }
            block.Code = new LinkedList<CodeLine>(bl2);
            return result;
        }
    }
}
