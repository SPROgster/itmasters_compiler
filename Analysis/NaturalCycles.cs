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
            List<int> linputs = CFG.GetInputs(l[n + 1]).Select(e => e.nBlock).ToList();
            HashSet<int> h = new HashSet<int>();
            h.Add(n);
            foreach (int item in linputs)
            {
                h.Add(item);
                DFS(CFG, l[item + 1], d, h);
                
            }
            return String.Join(" ", h) ;
        }
        public void DFS(ControlFlowGraph CFG, BaseBlock b, int d, HashSet<int> h)
        {
            List<BaseBlock> l = new List<BaseBlock>(CFG.GetBlocks());
            List<int> linputs = CFG.GetInputs(l[b.nBlock+ 1]).Select(e => e.nBlock).ToList();
            foreach (var item in linputs)
            {
                h.Add(item);
                if (item == d) return;
                DFS(CFG, l[item + 1], d, h);
            }
        }
    }
}
