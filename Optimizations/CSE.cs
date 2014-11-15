using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    class CSE
    {
        public static void cseOptimization(BaseBlock block)
        {            
            for (LinkedListNode<CodeLine> node0 = block.Code.Last; node0 != null; node0 = node0.Previous)
                for (LinkedListNode<CodeLine> node1 = node0.Next; node1 != null; node1 = node1.Next)
                {
                    if (node1.Value.Second == node0.Value.Second && node1.Value.Third == node0.Value.Third && node1.Value.Operation == node0.Value.Operation)
                    {
                        bool isOpt = true;
                        for (var tempNode = node0; tempNode != node1; tempNode = tempNode.Next)
                            if (tempNode.Value.First.Equals(node0.Value.Second) || tempNode.Value.First.Equals(node0.Value.Third))
                            {
                                isOpt = false;
                                break;
                            }
                        if (isOpt)
                        {
                            string tName = tempName + ++tempCounter;
                            block.Code.AddAfter(node0, new CodeLine(node0.Value.Label, node0.Value.First, tName, null, null));
                            node0.Value.First = tName;
                            node1.Value = new CodeLine(node1.Value.Label, node1.Value.First, tName, null, null);
                        }
                    }
                }
        }

        const string tempName = "_tCSE";
        static int tempCounter = 0;
    }
}
