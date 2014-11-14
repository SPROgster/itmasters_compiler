using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLang.MiddleEnd
{
    public enum SymbolKind { type, var }
    public enum CType { Int, Float, Double, Bool, None };

    public static class SymbolTable // Таблица символов
    {
        static SymbolTable()
        {
            // Initializing Symbol Table with all data types except for None
            vars = new List<Tuple<string, CType, SymbolKind>>();
            foreach (CType value in System.Enum.GetValues(typeof(CType)))
                if (value != CType.None)
                    vars.Add(new Tuple<string, CType, SymbolKind>(System.Enum.GetName(typeof(CType), value), value, SymbolKind.type));
        }

        public static List<Tuple<string, CType, SymbolKind>> vars;

        public static void Add(string name, CType type, SymbolKind kind)
        {
            //int Index = IndexOfIdent(name);
            //if (Index >= 0)
            //    if (vars[Index].Item3 == SymbolKind.var)
            //        throw new SemanticException("Переменная " + name + " уже определена");
            //    else
            //        throw new SemanticException("Тип " + name + " уже определён");
            //else
            vars.Add(new Tuple<string, CType, SymbolKind>(name, type, kind));
        }

        public static int IndexOfIdent(string id)
        {
            return vars.FindIndex(e => e.Item1 == id);
        }

        public static bool Contains(string id)
        {
            return IndexOfIdent(id) >= 0;
        }

        public static CType ParseType(string name)
        {
            switch (name)
            {
                case "int"      : return CType.Int;
                case "bool"     : return CType.Bool;
                case "float"    : return CType.Float;
                case "double"   : return CType.Double;
                default         : return CType.None;
            }
        }
    }
}
