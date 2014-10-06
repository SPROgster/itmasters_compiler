using System;
using System.Collections.Generic;

namespace SimpleParser
{
    public enum SymbolKind { type, var }
    public enum CType { Int, Double, Bool, None };

    public static class SymbolTable // Таблица символов
    {
        static SymbolTable()
        {
            vars = new List<Tuple<string, CType, SymbolKind>>();
            foreach (CType value in System.Enum.GetValues(typeof(CType)))
                if(value!=CType.None)
                    vars.Add(new Tuple<string, CType, SymbolKind>(System.Enum.GetName(typeof(CType), value), value, SymbolKind.type));
        }

        public static List<Tuple<string, CType, SymbolKind>> vars;

        public static void Add(string name, CType t, SymbolKind kind)
        {
            //int Index = IndexOfIdent(name);
            //if (Index >= 0)
            //    if (vars[Index].Item3 == SymbolKind.var)
            //        throw new SemanticException("Переменная " + name + " уже определена");
            //    else
            //        throw new SemanticException("Тип " + name + " уже определён");
            //else
                vars.Add(new Tuple<string, CType, SymbolKind>(name, t, kind));
        }

        public static int IndexOfIdent(string id)
        {
            return vars.FindIndex(e => e.Item1 == id);
        }

        public static bool Contains(string id)
        {
            return IndexOfIdent(id) >= 0;
        }
    }

    public class LexException : Exception
    {
        public LexException(string msg) : base(msg) { }
    }
    public class SyntaxException : Exception
    {
        public SyntaxException(string msg) : base(msg) { }
    }
    public class SemanticException : Exception
    {
        public SemanticException(string msg) : base(msg) { }
    }

    // Класс глобальных описаний и статических методов
    // для использования различными подсистемами парсера и сканера
    public static class ParserHelper
    {
        public static CType ParseType(string name)
        {
            switch (name)
            {
                case "int": return CType.Int;
                case "bool": return CType.Bool;
                default: return CType.None;
            }
        }
    }
}