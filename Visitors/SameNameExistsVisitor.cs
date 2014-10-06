﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramTree;

namespace SimpleLang.Visitors
{
    class SameNameExistsVisitor:AutoVisitor
    {
        public List<String> Errors = new List<string>();
        bool InVarDef = false;
        HashSet<String> Names = new HashSet<string>();

        public override void VisitVarDefNode (VarDefNode vd)
        {
            InVarDef = true;

            foreach (var id in vd.Idents)
                id.Visit(this);

            InVarDef = false;
        }

        public override void VisitIdNode(IdNode id)
        {
            if (InVarDef)
            {
                if (Names.Contains(id.Name))
                    Errors.Add("Повторное объявление переменной " + id.Name);
                else
                    Names.Add(id.Name);
            }
        }
    }
}
