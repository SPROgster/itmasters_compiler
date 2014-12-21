using System.Collections.Generic;
using System;
using SimpleLang.Visitors;
using SimpleParser;

namespace SimpleLang.MiddleEnd
{
    public enum BinOpType { None = 0, 
                            Plus, Minus, Div, Mult, 
                            Less, Greater, Equal, LEqual, GEqual, NEqual,
                          };

    public abstract class Node // базовый класс для всех узлов    
    {
        public abstract void Visit(Visitor v);
    }

    public abstract class ExprNode : Node // базовый класс для всех выражений
    {
        public abstract CType Type { get; }
    }

    public class IdNode : ExprNode
    {
        public string Name { get; set; }
        public IdNode(string name) { Name = name; }

        public override void Visit(Visitor v)
        {
            v.VisitIdNode(this);
        }

        public override CType Type
        {
            get { return SymbolTable.vars[Name].Item1; }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class IntNumNode : ExprNode
    {
        public int Num { get; set; }
        public IntNumNode(int num) { Num = num; }

        public override void Visit(Visitor v)
        {
            v.VisitIntNumNode(this);
        }

        public override CType Type
        {
            get { return CType.Int; }
        }

        public override string ToString()
        {
            return Num.ToString();
        }
    }

    public class FloatNumNode : ExprNode
    {
        public float Num { get; set; }
        public FloatNumNode(float num) { Num = num; }

        public override void Visit(Visitor v)
        {
            v.VisitFloatNumNode(this);
        }

        public override CType Type
        {
            get { return CType.Float; }
        }

        public override string ToString()
        {
            return Num.ToString();
        }
    }

    public class BoolNode : ExprNode
    {
        public bool Val { get; set; }
        public BoolNode(bool val) { Val = val; }

        public override void Visit(Visitor v)
        {
            v.VisitBoolNode(this);
        }

        public override CType Type
        {
            get { return CType.Bool; }
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    public class StringNode : ExprNode
    {
        public string Val { get; set; }
        public StringNode(string val) { Val = val; }

        public override void Visit(Visitor v)
        {
            v.VisitStringNode(this);
        }

        public override CType Type
        {
            get { return CType.String; }
        }

        public override string ToString()
        {
            return Val;
        }
    }

    public class BinOpNode : ExprNode
    {
        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }
        public BinOpType Op { get; set; }

        public BinOpNode(ExprNode left, ExprNode right, BinOpType operation)
        {
            Left = left;
            Right = right;
            Op = operation;
        }

        public override void Visit(Visitor v)
        {
            v.VisitBinOpNode(this);
        }

        public override CType Type
        {
            get
            {
                CType Op1 = Left.Type;
                CType Op2 = Right.Type;
                if (Op1 == CType.String || Op2 == CType.String)
                    return CType.String;
                else
                    return Op1 == Op2 ? Op1 : CType.None;
            }
        }

        public override string ToString()
        {
            return String.Format("({0} {1} {2})", Left.ToString(), Op, Right.ToString());
        }
    }

    public abstract class StatementNode : Node // базовый класс для всех операторов
    {
    }

    public class WriteNode : StatementNode
    {
        public ExprNode Expr;

        public WriteNode(ExprNode expression)
        {
            Expr = expression;
        }

        public override void Visit(Visitor v)
        {
            v.VisitWriteNode(this);
        }
    }

    public class VarDefNode : StatementNode
    {
        public List<IdNode> Idents = new List<IdNode>();
        public IdNode TypeIdent { get; set; }

        public VarDefNode(params IdNode[] idents)
        {
            foreach (IdNode id in idents)
                Idents.Add(id);
        }

        public void Add(IdNode id)
        {
            Idents.Add(id);
        }

        public override void Visit(Visitor v)
        {
            v.VisitVarDefNode(this);
        }
    }

    public class EmptyNode : StatementNode
    {
        public override void Visit(Visitor v)
        {
            v.VisitEmptyNode(this);
        }
    }

    public class AssignNode : StatementNode
    {
        public IdNode Id { get; set; }
        public ExprNode Expr { get; set; }
        public AssignNode(IdNode id, ExprNode expr)
        {
            Id = id;
            Expr = expr;
        }

        public override void Visit(Visitor v)
        {
            v.VisitAssignNode(this);
        }
    }

    public class CycleNode : StatementNode
    {
        public ExprNode Expr { get; set; }
        public StatementNode Stat { get; set; }

        public CycleNode(ExprNode expr, StatementNode stat)
        {
            Expr = expr;
            Stat = stat;
        }

        public override void Visit(Visitor v)
        {
            v.VisitCycleNode(this);
        }
    }

    public class WhileNode : StatementNode
    {
        public ExprNode Expr { get; set; }
        public StatementNode Stat { get; set; }

        public WhileNode(ExprNode expr, StatementNode stat)
        {
            Expr = expr;
            Stat = stat;
        }

        public override void Visit(Visitor v)
        {
            v.VisitWhileNode(this);
        }
    }

    public class IfNode : StatementNode
    {
        public ExprNode Expr { get; set; }
        public StatementNode StatIf { get; set; }
        public StatementNode StatElse { get; set; }

        public IfNode(ExprNode expr, StatementNode statIf, StatementNode statElse)
        {
            Expr = expr;
            StatIf = statIf;
            StatElse = statElse;
        }

        public IfNode(ExprNode expr, StatementNode statIf)
        {
            Expr = expr;
            StatIf = statIf;
            StatElse = new EmptyNode();
        }

        public override void Visit(Visitor v)
        {
            v.VisitIfNode(this);
        }
    }

    public class BlockNode : StatementNode
    {
        public List<StatementNode> StList = new List<StatementNode>();

        public BlockNode(StatementNode stat)
        {
            Add(stat);
        }

        public void Add(StatementNode stat)
        {
            StList.Add(stat);
        }

        public override void Visit(Visitor v)
        {
            v.VisitBlockNode(this);
        }
    }

}