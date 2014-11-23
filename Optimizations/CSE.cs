using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleLang.MiddleEnd;
using SimpleLang.Analysis;
using SimpleLang.Visitors;

namespace SimpleLang.Optimizations
{
    class CSE : LocalOptimization
    {
        /// <summary>
        /// Оптимизация общих подвыражений в блоке
        /// </summary>
        /// <param name="block"> Оптимизируемый блок </param>
        /// <returns> Были ли внесены какие-то изменения </returns>
        public bool Optimize(BaseBlock block)
        {
            return cseOptimization(block);
        }

        public static bool cseOptimization(BaseBlock block)
        {
            bool hasChanges = false;
            // поиск пары общих подвыражений
            for (LinkedListNode<CodeLine> node0 = block.Code.Last; node0 != null; node0 = node0.Previous)
                for (LinkedListNode<CodeLine> node1 = node0.Next; node1 != null; node1 = node1.Next)
                {
                    // если нашли...
                    if (node1.Value.Second == node0.Value.Second && node1.Value.Third == node0.Value.Third && node1.Value.BinOp == node0.Value.BinOp)
                    {
                        // ...то проверим, переопределяется ли переменная, присутствующая в общем подвыражении, на пути между общими подвыражениями
                        bool isOpt = true;
                        for (var tempNode = node0; tempNode != node1; tempNode = tempNode.Next)                            
                            if (tempNode.Value.First.Equals(node0.Value.Second) || tempNode.Value.First.Equals(node0.Value.Third))
                            {
                                isOpt = false;
                                break;
                            }
                        if (isOpt) // не переопределяется
                        {
                            string tName = SymbolTable.NextTemp();
                            Expression expr = new Expression(node0.Value.Second, node0.Value.Third, node0.Value.BinOp);                                                        
                            SymbolTable.vars.Add(new Tuple<string, CType, SymbolKind>(tName, expr.Type(), SymbolKind.var));
                            hasChanges = isOpt;
                            block.Code.AddAfter(node0, new CodeLine(node0.Value.Label, node0.Value.First, tName, null, BinOpType.None));
                            node0.Value.First = tName;
                            node1.Value = new CodeLine(node1.Value.Label, node1.Value.First, tName, null, BinOpType.None);
                        }
                    }
                }
            return hasChanges;
        }              
    }
}
