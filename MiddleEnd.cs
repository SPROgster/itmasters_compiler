﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiddleEnd
{
    //public class BaseBlock
    //{
    //    public LinkedList<CodeLine> Code = new LinkedList<CodeLine>();
    //    public LinkedList<BaseBlock> Input = new LinkedList<BaseBlock>();
    //    public LinkedList<BaseBlock> Output = new LinkedList<BaseBlock>();
    //}

    
    using BaseBlock = LinkedList<CodeLine>;

    public class ControlFlowGraph
    {
        public LinkedList<BaseBlock> Blocks;
        public Dictionary<BaseBlock, LinkedList<BaseBlock>> Inputs;
        public Dictionary<BaseBlock, LinkedList<BaseBlock>> Outputs;

        public ControlFlowGraph(LinkedList<CodeLine> Code)
        {
            //Итератор для прохода по трёхадресному коду
            LinkedListNode<CodeLine> Current = Code.First;
            //Помеченные команды
            Dictionary<string,CodeLine> Labeled = new Dictionary<string,CodeLine>();
            //Метки, на которые осуществлялся переход
            HashSet<string> UsedLabels = new HashSet<string>();
            //Здесь хранится информация о командах - лидерах
            HashSet<CodeLine> Leaders = new HashSet<CodeLine>();
            //Следующая команда - лидер
            bool NextIsLeader = true;
            //Первый проход - ищем лидеров
            while (Current != null)
            {
                if (NextIsLeader)
                {
                    Leaders.Add(Current.Value);
                    NextIsLeader = false;
                }
                if (Current.Value.Label != null)
                    Labeled[Current.Value.Label] = Current.Value;
                switch (Current.Value.Operation)
                {
                    case "g": 
                        UsedLabels.Add(Current.Value.First);
                        NextIsLeader = true;
                        break;
                    case "i": 
                        UsedLabels.Add(Current.Value.Second);
                        NextIsLeader = true;
                        break;
                }
                Current = Current.Next;
            }
            foreach (string Label in UsedLabels)
                Leaders.Add(Labeled[Label]);

            //Инициализируем структуры, описывающие граф
            Inputs = new Dictionary<BaseBlock, LinkedList<BaseBlock>>(Leaders.Count);
            Outputs = new Dictionary<BaseBlock, LinkedList<BaseBlock>>(Leaders.Count);
            Blocks = new LinkedList<BaseBlock>();

            //Второй проход - формируем базовые блоки
            BaseBlock CurrentBlock = new BaseBlock();
            CurrentBlock.AddFirst(Code.First.Value);
            Blocks.AddLast(CurrentBlock);
            //Сдвигаемся на следующую команду
            Current = Code.First.Next;
            //Будем сохранять информацию о том, какие блоки помечены метками и из каких блоков осуществляются переходы
            Dictionary<string, LinkedList<BaseBlock>> GotoLabelsDest = new Dictionary<string, LinkedList<BaseBlock>>();
            Dictionary<string,BaseBlock> GotoLabelsSrc = new Dictionary<string,BaseBlock>();
            while (Current != null)
            {
                //Если прошли один блок
                if (Leaders.Contains(Current.Value))
                {
                    //Определяемся с тем, связан ли он со следующим
                    if (Current.Previous.Value.Operation == "g")
                    {
                        if (!GotoLabelsDest.ContainsKey(Current.Previous.Value.First))
                            GotoLabelsDest[Current.Previous.Value.First] = new LinkedList<BaseBlock>();
                        GotoLabelsDest[Current.Previous.Value.First].AddLast(CurrentBlock);
                        CurrentBlock = new BaseBlock();
                        Blocks.AddLast(CurrentBlock);
                    }
                    else
                    {
                        if (Current.Previous.Value.Operation == "i")
                        {
                            if(!GotoLabelsDest.ContainsKey(Current.Previous.Value.Second))
                                GotoLabelsDest[Current.Previous.Value.Second] = new LinkedList<BaseBlock>();
                            GotoLabelsDest[Current.Previous.Value.Second].AddLast(CurrentBlock);
                        }
                        BaseBlock Tmp = new BaseBlock();
                        Outputs[CurrentBlock] = new LinkedList<BaseBlock>();
                        Outputs[CurrentBlock].AddLast(Tmp);
                        Inputs[Tmp] = new LinkedList<BaseBlock>();
                        Inputs[Tmp].AddLast(CurrentBlock);
                        CurrentBlock = Tmp;
                        Blocks.AddLast(CurrentBlock);
                    }
                    CurrentBlock.AddFirst(Current.Value);
                    if(Current.Value.Label!=null)
                        GotoLabelsSrc[Current.Value.Label] = CurrentBlock;
                }
                else
                    CurrentBlock.AddLast(Current.Value);
                Current = Current.Next;
            }
            //Проходим по блокам, помеченным метками
            foreach (var elem in GotoLabelsSrc)
                //Если на текущую метку осуществлялись переходы
                if(GotoLabelsDest.ContainsKey(elem.Key))
                    //Достраиваем связи
                    foreach (var dest in GotoLabelsDest[elem.Key])
                    {
                        if (!Outputs.ContainsKey(dest))
                            Outputs[dest] = new LinkedList<BaseBlock>();
                        Outputs[dest].AddLast(elem.Value);
                        if (!Inputs.ContainsKey(elem.Value))
                            Inputs[elem.Value] = new LinkedList<BaseBlock>();
                        Inputs[elem.Value].AddLast(dest);
                    }

        }
    }

    public class CodeLine
    {
        public string Label, First, Second, Third, Operation;

        public CodeLine(string lab, string fst, string snd, string thrd, string op)
        {
            Label = lab;
            First = fst;
            Second = snd;
            Third = thrd;
            Operation = op;
        }

        public override string ToString()
        {
            string ToReturn = Label + (Label != null ? ": " : " ");
            switch (Operation)
            {
                case "i": return ToReturn +  "if " + First + " goto " + Second;
                case "g": return ToReturn +  "goto " + First;
                default: return ToReturn + 
                    (First != null ? First + " := " + Second + " " + Operation + " " + Third : "");
            }
        }
    }

    public enum SymbolKind { type, var }
    public enum CType { Int, Double, Bool, None };

    public static class SymbolTable // Таблица символов
    {
        static SymbolTable()
        {
            vars = new List<Tuple<string, CType, SymbolKind>>();
            foreach (CType value in System.Enum.GetValues(typeof(CType)))
                if (value != CType.None)
                    vars.Add(new Tuple<string, CType, SymbolKind>(System.Enum.GetName(typeof(CType), value), value, SymbolKind.type));
        }

        public static List<Tuple<string, CType, SymbolKind>> vars;

        public static void Add(string name, CType t, SymbolKind kind)
        {
            //int Index = IndexOfIdent(name);
            //if (Index >= 0)
            //    if (vars[Index].Item3 == SymbolKind.var)
            //        throw new SemanticException("Переменная " + name + " уже определена");
            //    else
            //        throw new SemanticException("Тип " + name + " уже определён");
            //else
            vars.Add(new Tuple<string, CType, SymbolKind>(name, t, kind));
        }

        public static int IndexOfIdent(string id)
        {
            return vars.FindIndex(e => e.Item1 == id);
        }

        public static bool Contains(string id)
        {
            return IndexOfIdent(id) >= 0;
        }

        public static CType ParseType(string name)
        {
            switch (name)
            {
                case "int": return CType.Int;
                case "bool": return CType.Bool;
                default: return CType.None;
            }
        }
    }
}