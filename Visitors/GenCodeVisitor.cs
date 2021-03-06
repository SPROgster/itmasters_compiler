﻿using SimpleLang.MiddleEnd;
using System;
using System.Collections.Generic;

namespace SimpleLang.Visitors
{
    public class GenCodeVisitor : AutoVisitor
    {
        public LinkedList<CodeLine> Code =
            new LinkedList<CodeLine>();

        private Stack<string> NamesValuesStack = new Stack<string>();
        private Stack<CType> CTypeValuesStack = new Stack<CType>();
        private int LabelCounter = 0;
        private const string LabelName = "_l";

        private string NextLabel()
        {
            return LabelName + LabelCounter++;
        }

        public override void VisitAssignNode(AssignNode node)
        {
            if (node.Expr is BinOpNode)
            {
                (node.Expr as BinOpNode).Right.Visit(this);
                (node.Expr as BinOpNode).Left.Visit(this);
                node.Id.Visit(this);
                Code.AddLast(new CodeLine(null, NamesValuesStack.Pop(), NamesValuesStack.Pop(), NamesValuesStack.Pop(), (node.Expr as BinOpNode).Op));

                // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
                CTypeValuesStack.Pop(); CTypeValuesStack.Pop(); CTypeValuesStack.Pop();
            }
            else
            {
                node.Expr.Visit(this);
                node.Id.Visit(this);
                Code.AddLast(new CodeLine(null, NamesValuesStack.Pop(),
                             NamesValuesStack.Pop(), null, BinOpType.None));

                // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
                CTypeValuesStack.Pop(); CTypeValuesStack.Pop();
            }
        }

        public override void VisitIdNode(IdNode id)
        {
            // Определяем тип переменной
            if (!SymbolTable.Contains(id.Name))
                throw new Exception("Невозможно определить тип переменной " + id.Name + " , возможно она не описана в var");

            NamesValuesStack.Push(id.Name);
            CTypeValuesStack.Push(SymbolTable.vars[id.Name].Item1);
        }

        public override void VisitIntNumNode(IntNumNode num)
        {
            NamesValuesStack.Push(num.Num.ToString());
            CTypeValuesStack.Push(CType.Int);
        }

        public override void VisitFloatNumNode(FloatNumNode num)
        {
            NamesValuesStack.Push(num.Num.ToString());
            CTypeValuesStack.Push(CType.Float);
        }

        public override void VisitBoolNode(BoolNode val)
        {
            NamesValuesStack.Push(val.Val.ToString());
            CTypeValuesStack.Push(CType.Bool);
        }

        public override void VisitStringNode(StringNode val)
        {
            NamesValuesStack.Push(val.Val);
            CTypeValuesStack.Push(CType.String);
        }

        public override void VisitBinOpNode(BinOpNode binop)
        {
            binop.Right.Visit(this);
            binop.Left.Visit(this);
            string CurName = SymbolTable.NextTemp();

            // Какой же тут тип надо получить?
            CType resultType = SymbolTable.OpResultType(CTypeValuesStack.Pop(), CTypeValuesStack.Pop(), binop.Op);
            SymbolTable.vars.Add(CurName,new Tuple<CType, SymbolKind>(resultType, SymbolKind.var));

            Code.AddLast(new CodeLine(null, CurName,
                NamesValuesStack.Pop(), NamesValuesStack.Pop(), binop.Op));
            NamesValuesStack.Push(CurName);
            CTypeValuesStack.Push(resultType);
        }

        public override void VisitCycleNode(CycleNode c)
        {
            //Метка для кода после цикла
            string AfterCycleLabel = NextLabel();
            //Метка для проверки значения счётчика цикла
            string HeaderLabel = NextLabel();
            //Счётчик цикла
            string CounterVar = SymbolTable.NextTemp();
            SymbolTable.vars.Add(CounterVar, new Tuple<CType, SymbolKind>(CType.Int, SymbolKind.var));

            //Находим начальное значение счётчика
            c.Expr.Visit(this);

            //Завели переменную цикла
            Code.AddLast(new CodeLine(null, CounterVar,
                NamesValuesStack.Pop(), null, BinOpType.None));
            //Временная переменная для хранения результата сравнения
            string CompTemp = SymbolTable.NextTemp();
            SymbolTable.vars.Add(CompTemp, new Tuple<CType, SymbolKind>(CType.Bool, SymbolKind.var));
            //Надо ли выполнять тело?
            Code.AddLast(new CodeLine(HeaderLabel, CompTemp,
                CounterVar, "0", BinOpType.LEqual));
            Code.AddLast(new CodeLine(null, CompTemp,
                AfterCycleLabel, null, OperatorType.If));
            //Зафигачили в трёхадресный код тело цикла
            c.Stat.Visit(this);
            //Уменьшили значение счётчика на 1
            Code.AddLast(new CodeLine(null, CounterVar,
                CounterVar, "1", BinOpType.Minus));
            //Перешли к началу
            Code.AddLast(new CodeLine(null, HeaderLabel,
                null, null, OperatorType.Goto));
            //Дальше идёт код, на который переходим после окончания работы цикла
            Code.AddLast(new CodeLine(AfterCycleLabel, null,
                null, null, OperatorType.Nop));

            // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
            CTypeValuesStack.Pop();
        }

        public override void VisitWhileNode(WhileNode node)
        {
            string CondVariable = SymbolTable.NextTemp();
            string BodyLabel = NextLabel();
            string HeaderLabel = NextLabel();
            string AfterWhileLabel = NextLabel();

            SymbolTable.vars.Add(CondVariable,new Tuple<CType, SymbolKind>(CType.Bool, SymbolKind.var));

            Code.AddLast(new CodeLine(HeaderLabel, null, null, null, OperatorType.Nop));
            node.Expr.Visit(this);

            Code.AddLast(new CodeLine(null, CondVariable, NamesValuesStack.Pop(), null, BinOpType.None));
            Code.AddLast(new CodeLine(null, CondVariable, BodyLabel, null, OperatorType.If));
            Code.AddLast(new CodeLine(null, AfterWhileLabel, null, null, OperatorType.Goto));
            Code.AddLast(new CodeLine(BodyLabel, null, null, null, OperatorType.Nop));
            node.Stat.Visit(this);
            Code.AddLast(new CodeLine(null, HeaderLabel, null, null, OperatorType.Goto));
            Code.AddLast(new CodeLine(AfterWhileLabel, null, null, null, OperatorType.Nop));

            // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
            CTypeValuesStack.Pop();
        }

        public override void VisitIfNode(IfNode node)
        {
            string CondVariable = SymbolTable.NextTemp();
            string IfLabel = NextLabel();
            string AfterIfLabel = NextLabel();

            SymbolTable.vars.Add(CondVariable,new Tuple<CType, SymbolKind>(CType.Bool, SymbolKind.var));

            node.Expr.Visit(this);

            Code.AddLast(new CodeLine(null, CondVariable,
                NamesValuesStack.Pop(), null, BinOpType.None));
            Code.AddLast(new CodeLine(null, CondVariable,
                IfLabel, null, OperatorType.If));
            node.StatElse.Visit(this);
            Code.AddLast(new CodeLine(null, AfterIfLabel,
               null, null, OperatorType.Goto));
            Code.AddLast(new CodeLine(IfLabel, null,
               null, null, OperatorType.Nop));
            node.StatIf.Visit(this);
            Code.AddLast(new CodeLine(AfterIfLabel, null,
               null, null, OperatorType.Nop));

            // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
            CTypeValuesStack.Pop();
        }

        public override void VisitWriteNode(WriteNode node)
        {
            string WriteVariable = SymbolTable.NextTemp();

            SymbolTable.vars.Add(WriteVariable,new Tuple<CType,SymbolKind>(CType.String, SymbolKind.var));

            node.Expr.Visit(this);

            Code.AddLast(new CodeLine(null, WriteVariable,
                         NamesValuesStack.Pop(), null, BinOpType.None));
            Code.AddLast(new CodeLine(null, WriteVariable,
                         null, null, OperatorType.Write));

            // Снимаем со стеко типов столько же элементов, сколько сняли с стека имен
            CTypeValuesStack.Pop();
        }

        public void RemoveEmptyLabels()
        {
            RemoveEmptyLabels(Code);
        }

        /// <summary>
        /// Удаляет пустые метки из кода
        /// </summary>
        public static void RemoveEmptyLabels(LinkedList<CodeLine> code)
        {
            var Iterator = code.First;
            while (Iterator != null)
            {
                if (Iterator.Value.First == null && Iterator.Next != null)
                {
                    RenameLabels(code, Iterator.Next.Value.Label, Iterator.Value.Label);
                    Iterator.Next.Value.Label = Iterator.Value.Label;
                    Iterator = Iterator.Next;
                    code.Remove(Iterator.Previous);
                }
                else
                    Iterator = Iterator.Next;
            }
        }

        /// <summary>
        /// Заменяет метки со старым именем на новое имя
        /// </summary>
        private static void RenameLabels(LinkedList<CodeLine> inputCode, string oldName, string newName)
        {
            for (var elem = inputCode.First; elem != null; elem = elem.Next)
            {
                // замена метки
                if (elem.Value.Label != null && elem.Value.Label == oldName)
                    elem.Value.Label = newName;

                // замена в goto
                if (elem.Value.Operator == OperatorType.Goto)
                    if (elem.Value.First != null && elem.Value.First == oldName)
                        elem.Value.First = newName;

                // замена в if
                if (elem.Value.Operator == OperatorType.If)
                    if (elem.Value.Second != null && elem.Value.Second == oldName)
                        elem.Value.Second = newName;
            }
        }
    }
}