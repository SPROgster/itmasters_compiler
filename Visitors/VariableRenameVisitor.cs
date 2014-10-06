using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramTree;

namespace SimpleLang.Visitors
{
    class VariableRenameVisitor : AutoVisitor
    {
        public override void VisitIdNode(IdNode id)
        {
            id.Name = Char.ToUpper(id.Name[0])+id.Name.Substring(1);
        }
    }
}
