using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramTree;

namespace SimpleLang.Visitors
{
    class GenBlockVisitor: AutoVisitor
    {        
        protected LinkedList<Tuple<string, string, string, string, string>> tuples5;        
        protected static int cycle = 0;
        protected static int block = 0;

        public GenBlockVisitor()
        {           
            this.tuples5 = new LinkedList<Tuple<string, string, string, string, string>>();            
        }
        
        public override void VisitIdNode(IdNode id) 
        {            
        }
        
        public override void VisitIntNumNode(IntNumNode num) 
        {            
        }
        
        public override void VisitBinOpNode(BinOpNode binop) 
        {
            string lt = null, rt = null;
            lt = binop.Left.tmp;
            rt = binop.Right.tmp;            
            binop.Right.Visit(this);
            binop.Left.Visit(this);
            tuples5.AddLast(new Tuple<string, string, string, string, string>(null, binop.tmp, lt, rt, binop.Op.ToString()));                         
        }
        
        public override void VisitAssignNode(AssignNode a) 
        {
            string exp = a.Expr.tmp;
            a.Expr.Visit(this);
            tuples5.AddLast(new Tuple<string, string, string, string, string>(null, a.Id.Name, exp, null, ":="));                                             
        }        
        
        public override void VisitCycleNode(CycleNode c) 
        {
            string sufix = (++cycle).ToString();
            tuples5.AddLast(new Tuple<string, string, string, string, string>("iter" + sufix, null, null, null, null));
            c.Expr.Visit(this);
            tuples5.AddLast(new Tuple<string, string, string, string, string>(null, c.Expr.tmp, "exit" + sufix, null, "i"));            

            c.Stat.Visit(this);

            tuples5.AddLast(new Tuple<string, string, string, string, string>(null, "iter" + sufix, null, null, "g"));
            tuples5.AddLast(new Tuple<string, string, string, string, string>("exit" + sufix, null, null, null, null));
        }
        
        public override void VisitBlockNode(BlockNode bl) 
        {
            string sufix = (++block).ToString();
            tuples5.AddLast(new Tuple<string, string, string, string, string>("bb" + sufix, null, null, null, null));
            foreach (var st in bl.StList)
                st.Visit(this);            
            tuples5.AddLast(new Tuple<string, string, string, string, string>("eb" + sufix, null, null, null, null));            
        }
        
        public override void VisitWriteNode(WriteNode w) 
        {           
            w.Expr.Visit(this);            
            tuples5.AddLast(new Tuple<string, string, string, string, string>(null, w.Expr.tmp, null, null, "w"));
        }

        public override void VisitVarDefNode(VarDefNode w) { }        
        
        public virtual void PrintCommands()
        {
            int nCmd = 0;
            foreach (var t in this.tuples5)
            {
                Console.WriteLine("{0}: {1}", nCmd, t);
                ++nCmd;
            }
        }
        
    }    
}
