using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLang.MiddleEnd
{   
    public class BaseBlock: ICloneable
    {
        public LinkedList<CodeLine> Code = new LinkedList<CodeLine>();
        public int nBlock { get; set; }        

        public BaseBlock(int n = 0)
        {
            this.nBlock = n;
        }

        public void Add(CodeLine val)
        {
            Code.AddLast(val);
        }

        public object Clone()
        {
            var NewBlock = new BaseBlock();
            foreach(var cl in Code)
                NewBlock.Add((CodeLine)cl.Clone());
            return NewBlock;
        }

        public override string ToString()
        {
            return Code.Count>0 ? 
                Code.Select(e=>e.ToString()).Aggregate((a,b)=>a+Environment.NewLine+b) : "";
        }
    }

    public class ControlFlowGraph
    {
        private LinkedList<BaseBlock> Blocks;
        private Dictionary<BaseBlock, LinkedList<BaseBlock>> Inputs;
        private Dictionary<BaseBlock, LinkedList<BaseBlock>> Outputs;

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
                switch (Current.Value.Operator)
                {
                    case OperatorType.Goto: 
                        UsedLabels.Add(Current.Value.First);
                        NextIsLeader = true;
                        break;
                    case OperatorType.If: 
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
            int nBlock = 0;
            //Второй проход - формируем базовые блоки
            BaseBlock CurrentBlock = new BaseBlock(nBlock++);
            Blocks.AddLast(CurrentBlock);
            Current = Code.First;
            //Будем сохранять информацию о том, какие блоки помечены метками и из каких блоков осуществляются переходы
            Dictionary<string, LinkedList<BaseBlock>> GotoLabelsDest = new Dictionary<string, LinkedList<BaseBlock>>();
            Dictionary<string,BaseBlock> GotoLabelsSrc = new Dictionary<string,BaseBlock>();
            while (Current != null)
            {
                //Добавляем текущую команду в текущий блок
                CurrentBlock.Add(Current.Value);
                //Если это конец блока
                if (Current.Next == null || Leaders.Contains(Current.Next.Value))
                {
                    //Определяемся с тем, связан ли он со следующим
                    if (Current.Value.Operator == OperatorType.Goto)
                    {
                        if (!GotoLabelsDest.ContainsKey(Current.Value.First))
                            GotoLabelsDest[Current.Value.First] = new LinkedList<BaseBlock>();
                        GotoLabelsDest[Current.Value.First].AddLast(CurrentBlock);
                        if (Current.Next != null)
                        {
                            CurrentBlock = new BaseBlock(nBlock++);
                            Blocks.AddLast(CurrentBlock);
                        }
                    }
                    else
                    {
                        if (Current.Value.Operator == OperatorType.If)
                        {
                            if(!GotoLabelsDest.ContainsKey(Current.Value.Second))
                                GotoLabelsDest[Current.Value.Second] = new LinkedList<BaseBlock>();
                            GotoLabelsDest[Current.Value.Second].AddLast(CurrentBlock);
                        }
                        if (Current.Next != null)
                        {
                            BaseBlock Tmp = new BaseBlock(nBlock++);
                            Outputs[CurrentBlock] = new LinkedList<BaseBlock>();
                            Outputs[CurrentBlock].AddLast(Tmp);
                            Inputs[Tmp] = new LinkedList<BaseBlock>();
                            Inputs[Tmp].AddLast(CurrentBlock);
                            CurrentBlock = Tmp;
                            Blocks.AddLast(CurrentBlock);
                        }
                    }
                    if(Current.Next != null && Current.Next.Value.Label!=null)
                        GotoLabelsSrc[Current.Next.Value.Label] = CurrentBlock;
                }
                Current = Current.Next;
            }
            //Теперь для всех блоков точно существуют списки входов и выходов
            foreach (BaseBlock bl in Blocks)
            {
                if(!Inputs.ContainsKey(bl))
                    Inputs[bl] = new LinkedList<BaseBlock>();
                if (!Outputs.ContainsKey(bl))
                    Outputs[bl] = new LinkedList<BaseBlock>();
            }
            //Проходим по блокам, помеченным метками
            foreach (var elem in GotoLabelsSrc)
                //Если на текущую метку осуществлялись переходы
                if(GotoLabelsDest.ContainsKey(elem.Key))
                    //Достраиваем связи
                    foreach (var dest in GotoLabelsDest[elem.Key])
                    {
                        Outputs[dest].AddLast(elem.Value);
                        Inputs[elem.Value].AddLast(dest);
                    }
            //Создаём фиктивный блок "вход"
            BaseBlock EndBlock = new BaseBlock(int.MinValue);
            Inputs[EndBlock] = new LinkedList<BaseBlock>();
            Outputs[EndBlock] = new LinkedList<BaseBlock>();
            Outputs[EndBlock].AddLast(Blocks.First());
            Inputs[Blocks.First()].AddLast(EndBlock);
            Blocks.AddFirst(EndBlock);
            //Создаём фиктивный блок "выход"
            EndBlock = new BaseBlock(int.MaxValue);
            Inputs[EndBlock] = new LinkedList<BaseBlock>();
            Outputs[EndBlock] = new LinkedList<BaseBlock>();
            foreach(BaseBlock bl in Blocks)
                if (Outputs[bl].Count == 0)
                {
                    Outputs[bl].AddLast(EndBlock);
                    Inputs[EndBlock].AddLast(bl);
                }
            Blocks.AddLast(EndBlock);
        }

        //Возвращает список базовых блоков, являющихся предшественниками указанного
        public LinkedList<BaseBlock> GetInputs(BaseBlock block)
        {
            return Inputs[block];
        }
        //Возвращает список базовых блоков, являющихся дочерними для указанного
        public LinkedList<BaseBlock> GetOutputs(BaseBlock block)
        {
            return Outputs[block];
        }
        //Возвращает все блоки
        public LinkedList<BaseBlock> GetBlocks()
        {
            return Blocks;
        }

        //Возвращает блок "вход"
        public BaseBlock GetStart()
        {
            return Blocks.First();
        }

        //Возвращает блок "выход"
        public BaseBlock GetEnd()
        {
            return Blocks.Last();
        }        
    }
    // класс остовного дерева для графа потока управления
    public class SpanningTree
    {
        ControlFlowGraph gr;
        HashSet<BaseBlock> spTree;

        public SpanningTree(ControlFlowGraph gr)
        {
            this.gr = gr;
            this.spTree = new HashSet<BaseBlock>();
            // поиск в глубину
            foreach (BaseBlock u in gr.GetBlocks().Where(bl => !this.spTree.Contains(bl) && bl != gr.GetStart() && bl != gr.GetEnd()))
                DFS_Visit(u);
        }                
        /// <summary>
        /// посещение вершины в алгоритме поиска в глубину
        /// </summary>
        /// <param name="u">посещаемая вершина</param>
        void DFS_Visit(BaseBlock u)
        {                        
            spTree.Add(u);
            foreach (var child in this.gr.GetOutputs(u).Where(bl => !this.spTree.Contains(bl) && bl != this.gr.GetStart() && bl != this.gr.GetEnd()))                                   
                DFS_Visit(child);                         
        }
        // линейный вывод элементов остовного дерева на экран
        public void Print()
        {            
            foreach (BaseBlock block in this.spTree)
            {               
                Console.WriteLine("--- Узел {0} ---", block.nBlock);
                Console.WriteLine(block);
                Console.WriteLine("----------------");         
            }
        }
    }

    //Виды операторов, которые могут встретиться в нашем трёхадресном коде
    public enum OperatorType { Nop, Goto, If, Assign }

    public class CodeLine: ICloneable
    {
        public string Label, First, Second, Third;
        public BinOpType BinOp;
        public OperatorType Operator;

        public CodeLine(string lab, string fst, string snd, string thrd, BinOpType op)
        {
            Label = lab;
            First = fst;
            Second = snd;
            Third = thrd;
            Operator = OperatorType.Assign;
            BinOp = op;
        }

        public CodeLine(string lab, string fst, string snd, string thrd, 
            OperatorType op, BinOpType bop = BinOpType.None)
        {
            Label = lab;
            First = fst;
            Second = snd;
            Third = thrd;
            Operator = op;
            BinOp = bop;
        }

        public override string ToString()
        {
            string ToReturn = Label + (Label != null ? ": " : " ");
            switch (Operator)
            {
                case OperatorType.If: 
                    return ToReturn +  "if " + First + " goto " + Second;

                case OperatorType.Goto: 
                    return ToReturn +  "goto " + First;

                case OperatorType.Nop:
                    return ToReturn + "nop";

                //Присваивание
                default: 
                    return ToReturn + 
                        First + " := " + Second + " " + ((BinOp == BinOpType.None) ? "" : BinOp.ToString()) + " " + Third;
            }
        }

        public object Clone()
        {
            return new CodeLine(Label, First, Second, Third, BinOp);
        }
    }

}
