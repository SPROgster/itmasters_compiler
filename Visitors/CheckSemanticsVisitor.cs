using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Visitors
{
    public class CheckSemanticsVisitor:AutoVisitor
    {
        public List<String> Errors = new List<string>();
        // Flag for adding new vars in VisitIdNode, else - using
        bool InVarDef = false;
        private LinkedList<string> Names = new LinkedList<string>();

        public override void VisitVarDefNode (VarDefNode vd)
        {
            InVarDef = true;
            // Empty new names list 
            if (Names.Count() > 0)
                Names.Clear();
            
            // Adding new id's as names for vars
            foreach (var id in vd.Idents)
                id.Visit(this);
            InVarDef = false;

            // Adding new vars in SymbolTable
            foreach (var id in Names)
                SymbolTable.Add(id, SymbolTable.ParseType(vd.TypeIdent.Name), SymbolKind.var);
        }

        public override void VisitIdNode(IdNode id)
        {
            if (InVarDef)
            {
                if (SymbolTable.Contains(id.Name))
                    Errors.Add("Повторное объявление переменной " + id.Name);
                else
                    Names.AddLast(id.Name);
            }
            else
                if (!SymbolTable.Contains(id.Name))
                    Errors.Add("Переменная " + id.Name + " не была описана ранее");
        }

        public override void VisitBinOpNode(BinOpNode binop)
        {
            base.VisitBinOpNode(binop);
            if(binop.GetType()==CType.None)
                Errors.Add(String.Format("Несовместимые типы операндов в выражении {0}",binop.ToString()));
        }
    }
}
