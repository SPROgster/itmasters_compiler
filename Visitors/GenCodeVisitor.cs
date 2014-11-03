using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Visitors
{

    class GenCodeVisitor : AutoVisitor
    {
        public LinkedList<CodeLine> Code = 
            new LinkedList<CodeLine>();

        private Stack<string> NamesValuesStack = new Stack<string>();
        private int TempCounter = 0;
        private int LabelCounter = 0;
        private const string TempName = "_t";
        private const string LabelName = "_l";


        private string NextTemp()
        {
            while (SymbolTable.Contains(TempName + TempCounter))
                TempCounter++;
            return TempName + TempCounter++;
        }

        private string NextLabel()
        {
            return LabelName + LabelCounter++;
        }


        public override void VisitAssignNode(AssignNode node)
        {
            node.Expr.Visit(this);
            node.Id.Visit(this);
            Code.AddLast(new CodeLine(null, NamesValuesStack.Pop(),
                NamesValuesStack.Pop(), null, null));
        }

        public override void VisitIdNode(IdNode id)
        {
            NamesValuesStack.Push(id.Name);
        }

        public override void VisitIntNumNode(IntNumNode num)
        {
            NamesValuesStack.Push(num.Num.ToString());
        }

        public override void VisitBinOpNode(BinOpNode binop)
        {
            binop.Right.Visit(this);
            binop.Left.Visit(this);
            string CurName = NextTemp();
            Code.AddLast(new CodeLine(null,CurName,
                NamesValuesStack.Pop(),NamesValuesStack.Pop(),binop.Op.ToString()));
            NamesValuesStack.Push(CurName);
        }

        public override void VisitCycleNode(CycleNode c)
        {
            //Метка для кода после цикла
            string AfterCycleLabel = NextLabel();
            //Метка для проверки значения счётчика цикла
            string HeaderLabel = NextLabel();
            //Счётчик цикла
            string CounterVar= NextTemp();

            //Находим начальное значение счётчика
            c.Expr.Visit(this);

            //Завели переменную цикла
            Code.AddLast(new CodeLine(null, CounterVar,
                NamesValuesStack.Pop(), null, null));
            //Временная переменная для хранения результата сравнения
            string CompTemp = NextTemp();
            //Надо ли выполнять тело?
            Code.AddLast(new CodeLine(HeaderLabel, CompTemp,
                CounterVar, "0", BinOpType.LEqual.ToString()));
            Code.AddLast(new CodeLine(null, CompTemp,
                AfterCycleLabel, null, "i"));
            //Зафигачили в трёхадресный код тело цикла
            c.Stat.Visit(this);
            //Уменьшили значение счётчика на 1
            Code.AddLast(new CodeLine(null, CounterVar,
                CounterVar, "1", BinOpType.Minus.ToString()));
            //Перешли к началу
            Code.AddLast(new CodeLine(null, HeaderLabel,
                null, null, "g"));
            //Дальше идёт код, на который переходим после окончания работы цикла
            Code.AddLast(new CodeLine(AfterCycleLabel, null,
                null, null, "nop"));
        }

        public override void VisitWhileNode(WhileNode node)
        {
            string CondVariable = NextTemp();
            string BodyLabel = NextLabel();
            string HeaderLabel = NextLabel();
            string AfterWhileLabel = NextLabel();

            Code.AddLast(new CodeLine(HeaderLabel, null,
               null, null, "nop"));
            node.Expr.Visit(this);

            Code.AddLast(new CodeLine(null, CondVariable,
                NamesValuesStack.Pop(), null, null));
            Code.AddLast(new CodeLine(null, CondVariable,
                BodyLabel, null, "i"));
            Code.AddLast(new CodeLine(null, AfterWhileLabel,
               null, null, "g"));
            Code.AddLast(new CodeLine(BodyLabel, null,
               null, null, "nop"));
            node.Stat.Visit(this);
            Code.AddLast(new CodeLine(null, HeaderLabel,
               null, null, "g"));
            Code.AddLast(new CodeLine(AfterWhileLabel, null,
               null, null, "nop"));
        }

        public override void VisitIfNode(IfNode node)
        {
            string CondVariable = NextTemp();
            string IfLabel = NextLabel();
            string AfterIfLabel = NextLabel();

            node.Expr.Visit(this);

            Code.AddLast(new CodeLine(null, CondVariable,
                NamesValuesStack.Pop(), null, null));
            Code.AddLast(new CodeLine(null, CondVariable,
                IfLabel, null, "i"));
            node.StatElse.Visit(this);
            Code.AddLast(new CodeLine(null, AfterIfLabel,
               null, null, "g"));
            Code.AddLast(new CodeLine(IfLabel, null,
               null, null, "nop"));
            node.StatIf.Visit(this);
            Code.AddLast(new CodeLine(AfterIfLabel, null,
               null, null, "nop"));
        }
    }
}