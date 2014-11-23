using System;
using System.Linq;
using System.Collections.Generic;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    using ExprSet = SetAdapter<Expression>;

    public class Expression
    {
        public string Op1, Op2;
        public BinOpType Operation;

        public Expression(string op1, string op2, BinOpType oper)
        {
            Op1 = op1;
            Op2 = op2;
            Operation = oper;
        }

        public Expression()
        {
            Op1 = Op2 = null;
            Operation = BinOpType.None;
        }

        public override bool Equals(object obj)
        {
            if (obj is Expression)
            {
                Expression Other = (Expression)obj;
                return Other.Operation == this.Operation &&
                    (Other.Op1 == this.Op1 && Other.Op2 == this.Op2 ||
                    Other.Op1 == this.Op2 && Other.Op2 == this.Op1);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Op1.GetHashCode() * 3 + Op2.GetHashCode() * 7;
        }

        public override string ToString()
        {
            return Op1 + " " + Operation + " " + Op2;
        }
    }

    public class ExprGenKillContext : Context<Tuple<ExprSet, ExprSet>>
    {
        public ExprSet AllExprs { get; protected set; }

        public ExprGenKillContext(ControlFlowGraph cfg)
            : base(cfg)
        {
            AllExprs = new ExprSet();
            Dictionary<BaseBlock, HashSet<string>> BlockDefs = new Dictionary<BaseBlock, HashSet<string>>();
            foreach (BaseBlock block in cfg.GetBlocks())
            {
                BlockDefs[block] = new HashSet<string>();
                this[block] = new Tuple<ExprSet, ExprSet>(new ExprSet(), new ExprSet());
                var Current = block.Code.Last;
                for(int i=0;i<block.Code.Count;++i)
                {
                    if (Current.Value.Operator==OperatorType.Assign)
                    {
                        BlockDefs[block].Add(Current.Value.First);
                        if (Current.Value.BinOp != BinOpType.None)
                        {
                            var Element = new Expression(Current.Value.Second, Current.Value.Third, Current.Value.BinOp);
                            AllExprs.Add(Element);
                            if (!BlockDefs[block].Contains(Current.Value.Second) && !BlockDefs[block].Contains(Current.Value.Third))
                                this[block].Item1.Add(Element); //Добавляем элемент во множество gen
                        }
                    }
                    Current = Current.Previous;
                }
            }
            //Формируем множество kill
            foreach (Expression e in AllExprs)
                foreach (BaseBlock block in cfg.GetBlocks())
                    if (!this[block].Item1.Contains(e) &&
                        (BlockDefs[block].Contains(e.Op1) || BlockDefs[block].Contains(e.Op2)))
                        this[block].Item2.Add(e);
        }

        public ExprSet EGen(BaseBlock bl)
        {
            return this[bl].Item1;
        }

        public ExprSet EKill(BaseBlock bl)
        {
            return this[bl].Item2;
        }
    }

    public class AvailableExprsTransferFunction : InfoProvidedTransferFunction<Tuple<ExprSet, ExprSet>, ExprSet>
    {
        public AvailableExprsTransferFunction(Tuple<ExprSet, ExprSet> info)
            : base(info)
        { }

        public override ExprSet Transfer(ExprSet input)
        {
            return ExprSet.Union(Info.Item1, ExprSet.Subtract(input, Info.Item2));
        }
    }

    public class AvailableExprsAlgorithm : TopDownAlgorithm<Tuple<ExprSet, ExprSet>, ExprGenKillContext, ExprSet>
    {
        public AvailableExprsAlgorithm(ControlFlowGraph cfg)
            : base(cfg)
        {
            foreach (BaseBlock bl in cfg.GetBlocks())
            {
                In[bl] = new ExprSet();
                Out[bl] = new ExprSet();
                Func[bl] = new AvailableExprsTransferFunction(Cont[bl]);
            }
        }

        public override Tuple<Dictionary<BaseBlock, ExprSet>, Dictionary<BaseBlock, ExprSet>> Apply()
        {
            return base.Apply(new ExprSet(), Cont.AllExprs, Cont.AllExprs, ExprSet.Intersect);
        }
    }
}