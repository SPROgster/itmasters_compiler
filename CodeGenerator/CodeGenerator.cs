using System;
using System.Globalization;
using SimpleLang.MiddleEnd;

namespace SimpleLang.CodeGenerator
{
    interface CodeGenerator
    {
        string code(ControlFlowGraph CFG);
    }

	public class ValueParser
	{
		public CType type = CType.None;
		public ValueParser (string SymbolName)
		{
			// Boolean
			try
			{
				boolValue = bool.Parse(SymbolName);
				type = CType.Bool;
				return;
			}
			catch (FormatException) {}

			// Int
			try
			{
				intValue = int.Parse(SymbolName);
				type = CType.Int;
				return;
			}
			catch (FormatException) {}

			// Double
			try
			{
				doubleValue = double.Parse(SymbolName);
				type = CType.Double;
				return;
			}
			catch (FormatException) {}

			stringValue = SymbolName;
			type = CType.String;
		}

		public int ivalue() { return intValue; }
		public double dvalue() { return doubleValue; }
		public bool bvalue() { return boolValue; }
		public string svalue() { return stringValue; }

		//public enum CType { Int, Float, Double, Bool, String, None };
		protected int intValue;
		protected double doubleValue;
		protected bool boolValue;
		protected string stringValue;
	}
}