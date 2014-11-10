using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    class CSE
    {
        public static int t_id = 0;
        public static void CSE_inBBl(ref LinkedList<CodeLine> Code){
            for (LinkedListNode<CodeLine> node = Code.Last; node != null; node = node.Previous){
                for (LinkedListNode<CodeLine> node1 = node.Next; node1 != null; node1 = node1.Next){
                    if (node1.Value.First.Equals(node.Value.Second) || node1.Value.First.Equals(node.Value.Third)) break;
                    if (node1.Value.Second == node.Value.Second && node1.Value.Third == node.Value.Third && node1.Value.Operation == node.Value.Operation){
                        string lab = "_tcse" + t_id;
                        Code.AddBefore(node, new CodeLine(null, lab, node.Value.Second, node.Value.Third, node.Value.Operation));
                        node.Value = new CodeLine(null, node.Value.First, lab, null, null);
                        node1.Value = new CodeLine(null, node1.Value.First, lab, null, null);
                        t_id++;
                        break;
                    }
                }
            }
        }
    }
}
