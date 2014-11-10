using System;
using System.Collections.Generic;
using MiddleEnd;
using DefUse;

namespace CleanDead
{
    using BaseBlock = LinkedList<CodeLine>;

    public class CleanDead
    {
        public static void cleanBlock(BaseBlock block)
        {
            var list = DeadOrAlive.GenerateDefUse(new List<CodeLine>(block));

            for (var p = block.First; p != null; p = p.Next)
            {
                foreach (Tuple<string, string, int> def in list)
                {
                    if (!DeadOrAlive.IsAlive(block, def.Item1, def.Item3))
                        block.Remove(p);
                }
            }
        }
    }
}
