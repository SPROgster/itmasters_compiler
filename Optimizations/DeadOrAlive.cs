using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiddleEnd;

namespace DefUse
{
    using BaseBlock = LinkedList<CodeLine>;
    class DeadOrAlive
    {
        public static List<Tuple<string, string, int>> GenerateDefUse(List<CodeLine> l)
        {
            List<Tuple<string, string, int>> l2 = new List<Tuple<string, string, int>>();
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].First != null && SymbolTable.Contains(l[i].First))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].First, "def", i));
                }
                if (l[i].Second != null && SymbolTable.Contains(l[i].Second))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].Second, "use", i));
                }
                if (l[i].Third != null && SymbolTable.Contains(l[i].Third))
                {
                    l2.Add(new Tuple<string, string, int>(l[i].Third, "use", i));
                }
            }
            return l2;
        }

        public static bool IsAlive(BaseBlock bl, string id, int line)
        {
            List<CodeLine> bl2 = new List<CodeLine>(bl);
            List<Tuple<string, string, int>> l2=GenerateDefUse(bl2);
            var l3=l2.Where(t=>t.Item1.Equals(id));
            l3.Reverse();
            foreach (Tuple<string, string, int> item in l3)
            {
                Console.WriteLine(item);
            }

            return true;

        }
    }
}
