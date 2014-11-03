using System;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{

    internal class DeadOrAlive
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
            List<CodeLine> bl2 = new List<CodeLine>(bl.Code);
            bool alive;
            if (id.Length >= 2 && id.Substring(0, 2).Equals("_t"))
            {
                alive = false;
            }
            else
            {
                alive = true;
            }

            for (int i = bl2.Count - 1; i >= 0; i--)
            {
                if (bl2[i].Third != null && bl2[i].Third.Equals(id))
                {
                    alive = true;
                }
                if (bl2[i].Second != null && bl2[i].Second.Equals(id))
                {
                    alive = true;
                }                
                if (bl2[i].First != null && bl2[i].First.Equals(id))
                {
                    alive = false;
                }
                if (i == line)
                {
                    return alive;
                }
            }
            return alive;
        }
    }
}