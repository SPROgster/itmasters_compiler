using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    
    // класс для причёсывния трёхадресного кода
    class Correct3AddressCode
    {
        /// <summary>
        /// Удаляет пустые метки из кода
        /// </summary>
        public static void RemoveEmptyLabel(ref LinkedList<CodeLine> code)
        {
            var Iterator = code.First;
            while (Iterator != null)
            {
                if (Iterator.Value.First == null && Iterator.Next != null)
                {
                    Correct3AddressCode.RenameLabeles(ref code, Iterator.Next.Value.Label, Iterator.Value.Label);                    
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
        private static void RenameLabeles(ref LinkedList<CodeLine> inputCode, string oldName, string newName)
        {
            for (var elem = inputCode.First; elem != null; elem = elem.Next)
            {
                // замена метки
                if (elem.Value.Label != null && elem.Value.Label == oldName)
                    elem.Value.Label = newName;

                // замена в goto
                if (elem.Value.Operation == "g")                
                    if (elem.Value.First != null && elem.Value.First == oldName)
                        elem.Value.First = newName;
                
            }
        }

    }
}
