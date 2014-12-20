using SimpleLang.MiddleEnd;
using System.Collections.Generic;

namespace SimpleLang.Optimizations
{
    public class Fold : LocalOptimization
    {
        public string GetName()
        {
            return "Свертка констант и алгебраических тождеств";
        }

        public bool Optimize(BaseBlock block)
        {
            return fold(ref block.Code);
        }

        // Основная функция свертки
        public static bool fold(ref LinkedList<CodeLine> gcv)
        {
            bool sameness = FoldSameness(ref gcv); // Сворачиваем алг тождества
            // В цикле проходим по таблице трехадресного кода
            // Одна функция сворачивает бинарные операции на константах в правой части
            // Другая функция подставляет инициализации переменной константами
            // В выражения-вхождения данной переменной.
            bool calc_fold = false;
            while (true)
            {
                bool hadRightConst = foldRightConst(ref gcv);
                bool hadRightSemiConst = foldRightSemiConst(ref gcv);
                if (!hadRightConst && !hadRightSemiConst)
                    break;
                else
                    calc_fold = true;
            }
            return (sameness || calc_fold);
        }

        // Функция сворачивающая константы
        // Функция проходит по объявлениям перменным, и если они инициализированы
        // константой, то функция меняет вхождения переменной до
        // Следующего переопределения переменной
        private static bool foldRightSemiConst(ref LinkedList<CodeLine> gcv)
        {
            bool has_const = false;
            for (var elem = gcv.First; elem != null; elem = elem.Next)
            {
                CodeLine cl = elem.Value;
                if (cl.Third == null && cl.BinOp == BinOpType.None)
                {
                    int second;
                    bool second_int = int.TryParse(cl.Second, out second);
                    if (second_int)
                    {
                        for (var _elem = elem; _elem != null; _elem = _elem.Next)
                        {
                            if (_elem != elem && _elem.Value.First == cl.First)
                                break;
                            if (_elem.Value.Second == cl.First)
                            {
                                _elem.Value.Second = cl.Second;
                                has_const = true;
                            }
                            if (_elem.Value.Third == cl.First)
                            {
                                _elem.Value.Third = cl.Second;
                                has_const = true;
                            }
                        }
                    }
                }
            }
            return has_const;
        }

        // Функция сворачивает выражения, имеющие в правой части
        // Конструкции вида <const>{+,-,*}<const>
        private static bool foldRightConst(ref LinkedList<CodeLine> gcv)
        {
            bool has_const = false;
            foreach (CodeLine cl in gcv)
            {
                int third = 0;
                int second = 0;
                bool second_int = int.TryParse(cl.Second, out second);
                bool third_int = int.TryParse(cl.Third, out third);
                if (second_int && third_int)
                {
                    if (cl.BinOp == BinOpType.Plus)
                        cl.Second = (third + second).ToString();
                    else if (cl.BinOp == BinOpType.Mult)
                        cl.Second = (third * second).ToString();
                    else if (cl.BinOp == BinOpType.Minus)
                        cl.Second = (second - third).ToString();
                    else
                        continue;
                    cl.Third = null;
                    cl.BinOp = BinOpType.None;
                    has_const = true;
                }
            }
            return has_const;
        }

        // Функция меняющая трехадресный код
        // В случае операции сложения и операнда 0
        private static bool checkPlusOrMinus(CodeLine cl)
        {
            bool changed = false;
            if (cl.Second == "0")
            {
                if (cl.BinOp == BinOpType.Minus)
                    return changed;
                else
                    cl.Second = cl.Third;
                cl.Third = null;
                cl.BinOp = BinOpType.None;
                changed = true;
            }
            if (cl.Third != null && cl.Third == "0")
            {
                cl.Third = null;
                cl.BinOp = BinOpType.None;
                changed = true;
            }
            return changed;
        }

        // Функция меняющая трехадресный код
        // В случае операции умножить и операнда 1 или 0
        private static bool checkMult(CodeLine cl)
        {
            bool changed = false;
            if (cl.Second == "1" && cl.Third != null)
            {
                cl.Second = cl.Third;
                cl.Third = null;
                cl.BinOp = BinOpType.None;
                changed = true;
            }
            if (cl.Third != null && cl.Third == "1")
            {
                cl.Third = null;
                cl.BinOp = BinOpType.None;
                changed = true;
            }
            if (cl.Second == "0" || (cl.Third != null && cl.Third == "0"))
            {
                cl.Second = "0";
                cl.Third = null;
                cl.BinOp = BinOpType.None;
                changed = true;
            }
            return changed;
        }

        // Функция, которая сворачивает алгебраические тождества
        private static bool FoldSameness(ref LinkedList<CodeLine> gcv)
        {
            bool folded = false;
            foreach (CodeLine cl in gcv)
            {
                int third = 0;
                int second = 0;
                bool second_int = int.TryParse(cl.Second, out second);
                bool third_int = int.TryParse(cl.Third, out third);
                if (second_int || third_int)
                {
                    bool iter_changed = false;
                    switch (cl.BinOp)
                    {
                        case BinOpType.Plus:
                            iter_changed = checkPlusOrMinus(cl);
                            break;

                        case BinOpType.Minus:
                            iter_changed = checkPlusOrMinus(cl);
                            break;

                        case BinOpType.Mult:
                            iter_changed = checkMult(cl);
                            break;

                        default:
                            break;
                    }
                    if (iter_changed)
                        folded = true;
                }
            }
            return folded;
        }
    }
}