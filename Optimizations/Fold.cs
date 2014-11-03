using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    class Fold
    {
        // Основная функция свертки
        public static void fold(ref LinkedList<CodeLine> gcv)
        {
            FoldSameness(ref gcv); // Сворачиваем алг тождества
            // В цикле проходим по таблице трехадресного кода
            // Одна функция сворачивает бинарные операции на константах в правой части
            // Другая функция подставляет инициализации переменной константами
            // В выражения-вхождения данной переменной.
            while (true)
            {
                bool hadRightConst = foldRightConst(ref gcv);
                bool hadRightSemiConst = foldRightSemiConst(ref gcv);
                if (!hadRightConst && !hadRightSemiConst)
                    break;
            }
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
                if (cl.Third == null && cl.Operation == null)
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
                    if (cl.Operation == "Plus")
                        cl.Second = (third + second).ToString();
                    else if (cl.Operation == "Mult")
                        cl.Second = (third * second).ToString();
                    else if (cl.Operation == "Minus")
                        cl.Second = (second - third).ToString();
                    else
                        continue;
                    cl.Third = null;
                    cl.Operation = null;
                    has_const = true;
                }
            }
            return has_const;
        }

        // Функция меняющая трехадресный код
        // В случае операции умножить и операнда 0
        private static void checkPlusOrMinus(CodeLine cl)
        {
            if (cl.Second == "0")
            {
                if (cl.Operation == "Minus")
                    return;
                else
                    cl.Second = cl.Third;
                cl.Third = null;
                cl.Operation = null;
            }
            if (cl.Third != null && cl.Third == "0")
            {
                cl.Third = null;
                cl.Operation = null;
            }
        }

        // Функция меняющая трехадресный код
        // В случае операции умножить и операнда 1 или 0
        private static void checkMult(CodeLine cl)
        {
            if (cl.Second == "1" && cl.Third != null)
            {
                cl.Second = cl.Third;
                cl.Third = null;
                cl.Operation = null;
            }
            if (cl.Third != null && cl.Third == "1")
            {
                cl.Third = null;
                cl.Operation = null;
            }
            if (cl.Second == "0" || (cl.Third != null && cl.Third == "0"))
            {
                cl.Second = "0";
                cl.Third = null;
                cl.Operation = null;
            }
        }
        
        // Функция, которая сворачивает алгебраические тождества
        private  static void FoldSameness(ref LinkedList<CodeLine> gcv)
        {
            foreach (CodeLine cl in gcv)
            {
                int third = 0;
                int second = 0;
                bool second_int = int.TryParse(cl.Second, out second);
                bool third_int = int.TryParse(cl.Third, out third);
                if (second_int || third_int)
                {
                    switch (cl.Operation)
                    {
                        case "Plus":
                            checkPlusOrMinus(cl);
                            break;
                        case "Minus":
                            checkPlusOrMinus(cl);
                            break;
                        case "Mult":
                            checkMult(cl);
                            break;
                        default:
                            break;
                    }
                }
            }
        }




    }
}
