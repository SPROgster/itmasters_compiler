using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Visitors
{
    class CheckVariablesVisitor:AutoVisitor
    {
        public List<String> Errors = new List<string>();
        bool InVarDef = false;
        private LinkedList<string> Names = new LinkedList<string>();

        public override void VisitVarDefNode (VarDefNode vd)
        {
            InVarDef = true;
            foreach (var id in vd.Idents)
                id.Visit(this);
            InVarDef = false;
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
    }
}
