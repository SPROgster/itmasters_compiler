using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLang.MiddleEnd
{
    public enum SymbolKind { type, var, keyword}
    public enum CType { Int, Float, Double, Bool, String, None };

    public static class SymbolTable // Таблица символов
    {
        public static string[] KeyWords = {"begin", "end", "cycle", "while", "if", "else",
                                              "write", "var", "true", "false" };

        static SymbolTable()
        {
            // Initializing Symbol Table with all data types except for None
            vars = new List<Tuple<string, CType, SymbolKind>>();
            foreach (CType value in System.Enum.GetValues(typeof(CType)))
                if (value != CType.None)
                    vars.Add(new Tuple<string, CType, SymbolKind>(System.Enum.GetName(typeof(CType), value), CType.None, SymbolKind.type));
            foreach (string s in KeyWords)
                vars.Add(new Tuple<string, CType, SymbolKind>(s,CType.None,SymbolKind.keyword));
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
				case "string"   : return CType.String;
                default         : return CType.None;
            }
        }

        /// <summary>
        /// Определяет какого типа должен быть результат операции по двум операторам
        /// </summary>
        /// <param name="op1">Первый операнд</param>
        /// <param name="op2">Второй операнд</param>
        /// <param name="op">Операция</param>
        /// <returns>CType результата операции</returns>
        public static CType OpResultType(CType op1, CType op2, BinOpType op)
        {
            if (op2 == CType.None)
                return op1;

            if (op == BinOpType.Equal || op == BinOpType.GEqual || op == BinOpType.Greater ||
                op == BinOpType.LEqual || op == BinOpType.Less || op == BinOpType.NEqual)
                return CType.Bool;

			if ((op1 == CType.String) || (op2 == CType.String))
				return CType.String;

            if ((op1 == CType.Double) || (op2 == CType.Double))
                return CType.Double;

            if ((op1 == CType.Float) || (op2 == CType.Float))
                return CType.Float;

            if ((op1 == CType.Int) || (op2 == CType.Int))
                return CType.Int;

            if ((op1 == CType.Bool) || (op2 == CType.Bool))
                return CType.Bool;

            return CType.None;
        }

        static int TempCounter = 0;
        const string TempName = "_t";
        /// <summary>
        /// Возвращает следующую неиспользуемую временную переменную
        /// </summary>
        /// <returns>следующая неиспользуемая временная переменная</returns>
        public static string NextTemp()
        {
            while (SymbolTable.Contains(TempName + TempCounter))
                TempCounter++;
            return TempName + TempCounter++;
        }
    }
}
