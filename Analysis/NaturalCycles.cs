using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{

    public class NaturalCycles
    {
        public string GetLoop(ControlFlowGraph CFG, int n, int d)
        {
            List<BaseBlock> l = new List<BaseBlock>(CFG.GetBlocks());
            HashSet<int> h = new HashSet<int>();
            h.Add(n);
            DFS(CFG, l[n + 1], d, h);
            return String.Join(" ", h) ;
        }
        public void DFS(ControlFlowGraph CFG, BaseBlock b, int d, HashSet<int> h)
        {
            List<BaseBlock> l = new List<BaseBlock>(CFG.GetBlocks());
            foreach (var item in CFG.GetInputs(l[b.nBlock+ 1]).Select(e => e.nBlock).ToList())
            {
                h.Add(item);
                if (item == d) return;
                DFS(CFG, l[item + 1], d, h);
            }
        }
    }
}
