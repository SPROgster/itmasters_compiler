using System;
using System.Collections.Generic;
using SimpleLang.MiddleEnd;
using SimpleLang.Analysis;
using System.Linq;

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
            foreach (var item in list.Where(x=>x.Item2.Equals("def")))
            {
                if (DeadOrAlive.IsDead(block, item.Item1, item.Item3))
                {
                    toDelete.Add(item.Item3);
                    result = true;
                }
            }
            toDelete.Sort();
            toDelete.Reverse();
            foreach (var item in toDelete)
            {
                bl2.RemoveAt(item);
            }
            block.Code = new LinkedList<CodeLine>(bl2);
            return result;
        }


        public string GetName()
        {
            return "Удаление мёртвого кода";
        }
    }
}
