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
				boolValue = Convert.ToBoolean(SymbolName);
				type = CType.Bool;
				return;
			}
			catch (FormatException) {}

			// Int
			try
			{
				intValue = Convert.ToInt32(SymbolName);
				type = CType.Int;
				return;
			}
			catch (FormatException) {}

			// Double
			try
			{
				NumberFormatInfo nfi = new NumberFormatInfo();
				nfi.NumberDecimalSeparator = ".";
				doubleValue = Convert.ToDouble(SymbolName, nfi);
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