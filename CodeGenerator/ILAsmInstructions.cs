using System;
using System.Collections.Generic;
using SimpleLang.MiddleEnd;

using IndexType = System.Tuple<int, SimpleLang.MiddleEnd.CType>;


namespace SimpleLang.CodeGenerator
{
    /// <summary>
    ///     Класс, с помощью которого трехадресный код переводится в одну или несколько IL инструкций.
    /// Перед использованием необходимо заполнить Local переменную - список локальных переменных в области видимости
    /// </summary>
    public static class IL
    {
        /// <summary>
        /// Спецификаторы типов операндов при инструкциях
        /// </summary>
        public static Dictionary<CType, string> ILOpType = new Dictionary<CType, string>();

        public const int MaxStackSize = 2; // Для LEqual и других

        /// <summary>
        /// Заполняем значениями спецификаторы типов операндов при инструкциях
        /// </summary>
        static IL()
        {
            ILOpType.Add(CType.Int,     "i4");
            ILOpType.Add(CType.Bool,    "i4");
            ILOpType.Add(CType.Float,   "r4");
            ILOpType.Add(CType.Double,  "r8");
        }

        /// <summary>
        /// Описание локальных переменных. Должно быть заполнено или же будет ошибка
        /// </summary>
        public static SimpleLang.CodeGenerator.ILAsm.ILLocal Local;

        /// <summary>
        /// IL команда помещения в стек IDnode (чтение значения переменной или константы)
        /// </summary>
        /// <param name="SymbolName">Имя символа</param>
        /// <returns>IL инструкция</returns>
		public static string pushId(string SymbolName)
        {
            IndexType Operand = Local[SymbolName];

            // Если истина, то у нас присваивание константе
            if (Operand == null)
			{
				ValueParser value = new ValueParser(SymbolName);

				switch (value.type) 
				{
				case CType.Bool:
					return "ldc.i4 " + ((value.bvalue()) ? "1" : "0") + Environment.NewLine;

				case CType.Double:
					return "ldc." + ILOpType[CType.Double] + " " + SymbolName + Environment.NewLine;

				case CType.Int:
					return "ldc." + ILOpType[CType.Int] + " " + SymbolName + Environment.NewLine;

				default:
					return "ldstr \"" + SymbolName + "\"" + Environment.NewLine;
				}
			}
            // Иначе кладем на стек по номеру элемента
            else
                return "ldloc " + Operand.Item1.ToString() + Environment.NewLine;
        }

        /// <summary>
        /// IL команда извлечения из стека IDnode (запись в переменнную)
        /// </summary>
        /// <param name="SymbolName">Имя символа</param>
        /// <returns>IL инструкция</returns>
		public static string popId(string SymbolName)
        {
            IndexType Operand = Local[SymbolName];

            if (Operand == null)
                throw new Exception("Попытка присвоить значение не l-значению или переменная не найдена " + SymbolName);

            return "stloc " + Operand.Item1.ToString() + Environment.NewLine;
        }

        /// <summary>
        /// Генерация одной или несколько IL инструкций по line трехадресному коду
        /// </summary>
        /// <param name="line">Строка трехадресного кода</param>
        /// <returns>IL инструкция или последовательность сиих инструкций</returns>
        public static string code(CodeLine line)
        {
            string code = "";
            if (line.Label != null)
                code += line.Label + ": ";

            // Если первый адрес не задан, то это у нас может быть только nop. Или еще какая-то хрень
            if (line.First == null)
            {
                code += "nop" + Environment.NewLine;
                return code;
            }

            switch (line.Operator)
            {
                case OperatorType.Assign:
                    switch (line.BinOp)
                    {
                        // Присваивание вида a:=b
                        case BinOpType.None:
							//IndexType Operand = Local[line.First];
							code += pushId(line.Second);
                            code += popId(line.First);

                            return code;
                        // Операция "+"
                        case BinOpType.Plus:
							//IndexType Operand = Local[line.First];
							code += pushId(line.Second);
							code += pushId(line.Third);

                            code += "add" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "-"
                        case BinOpType.Minus:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "sub" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "/"
                        case BinOpType.Div:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "div" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "*"
                        case BinOpType.Mult:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "mul" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "<"
                        case BinOpType.Less:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "clt" + Environment.NewLine;
                            code += popId(line.First);
                            return code;

                        // Операция ">"
                        case BinOpType.Greater:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "cgt" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "="
                        case BinOpType.Equal:
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            code += "ceq" + Environment.NewLine;
                            code += popId(line.First);
                            return code;

                        //
                        //  ВНИМАНИЕ!!! КОСТЫЛЬ!!! ОТ мелкософта
                        //
                        // Операция "<="
                        case BinOpType.LEqual:
                            // Проверяем на <=
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            // Т.к. в IL нету вычисления <=, то сравниваем на >
                            code += "cgt" + Environment.NewLine;

                            // Кладем 0
                            code += pushId("0");

                            // Проверяем на равенство с 0 (Логическое не). Это такой костыль от Мелкософта
                            code += "ceq" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция ">="
                        case BinOpType.GEqual:
                            // Проверяем на >=
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            // Т.к. в IL нету вычисления >=, то сравниваем на <
                            code += "clt" + Environment.NewLine;

                            // Кладем 0
                            code += pushId("0");

                            // Проверяем на равенство с 0 (Логическое не). Это такой костыль от Мелкософта
                            code += "ceq" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        // Операция "<>"
                        case BinOpType.NEqual:
                            // Проверяем на неравенство
							code += pushId(line.Second);
                            code += pushId(line.Third);

                            // Т.к. в IL нету вычисления неравенства, то сравниваем на равенство
                            code += "ceq" + Environment.NewLine;

                            // Кладем 0
                            code += pushId("0");

                            // Проверяем на равенство с 0 (Логическое не). Это такой костыль от Мелкософта
                            code += "ceq" + Environment.NewLine;

                            code += popId(line.First);
                            return code;

                        default:
                            throw new Exception("Неизвестная новая операция " + line.BinOp.ToString());
                    }

                case OperatorType.Goto:
                    code += "br " + line.First + Environment.NewLine;
                    return code;

                case OperatorType.If:
                    code += pushId(line.First);
                    code += "brtrue " + line.Second;
                    return code;
                default:
                    throw new Exception("Неизвестный новый оператор " + line.Operator.ToString());
            }
               // Это понадобится позже
                // call       instance string [mscorlib]System.Int32::ToString()
                // call       void [mscorlib]System.Console::WriteLine(string)
                // call       void [mscorlib]System.Console::WriteLine(int32)

                /*
                 * .assembly extern mscorlib {} 
                 * .assembly hello {}
                 * .method static public void main() cil managed
                 * {
                 *      .entrypoint
                 *      .maxstack 1 
                 *      ldstr "Hello world!" 
                 *      call void [mscorlib]System.Console::WriteLine(class System.String)
                 *      ret
                 * }
                 * 
                 * */
        }
    }
}
