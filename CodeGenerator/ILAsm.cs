using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

using IndexType = System.Tuple<int, SimpleLang.MiddleEnd.CType>;



namespace SimpleLang.CodeGenerator
{
    public class ILAsm : CodeGenerator
    {
        // Настройка, выводить ли в сгенерированный код трехадресный код
        const bool comment3AdrCode = true;

        public static ILLocal Local = new ILLocal();

        /// <summary>
        /// Класс локальных переменных
        /// </summary>
        public class ILLocal
        {
            /// <summary>
            /// Ассоциативный массив типов, ставивший в соответствие IL типы
            /// </summary>
            private Dictionary<CType, string> ILType = new Dictionary<CType, string>();
            /// <summary>
            /// Таблица имя переменной - индекс локальной переменной
            /// </summary>
            private Dictionary<string, IndexType> varsIndex = new Dictionary<string, IndexType>();
            /// <summary>
            /// Внутреняя часть дерективы со списком локальных переменных
            /// </summary>
            private string localDirectiveList;

            public ILLocal()
            {
                ILType.Add(CType.Int, "int32");
                ILType.Add(CType.Bool, "int32");
                ILType.Add(CType.Float, "float32");
                ILType.Add(CType.Double, "float64");

                processLocals();
            }

            /// <summary>
            /// Генерирует дерективу .locals для записи внутри метода с описанием локальных переменных
            /// </summary>
            public string generateDerective()
            {
                // Запихиваем описание переменных в директиву
                if (varsIndex.Count > 0)
                    return ".locals init (" + localDirectiveList + ")" + Environment.NewLine;
                else
                    return "";
            }

            private void processLocals()
            {
                varsIndex.Clear();
                int i = 0;
                localDirectiveList = "";

                var Types = from ids in MiddleEnd.SymbolTable.vars
                            where ids.Item3 != SymbolKind.type
                            select ids;

                foreach (var id in Types)
                {
                    // Для ILType[id.Item2] 
                    try
                    {
                        // Строка описания локальной переменной при дириктиве .local
                        string varText = "[" + i.ToString() + "] " + ILType[id.Item2] + " " + id.Item1;

                        // Добавление запятой, в случае, если это не первый элемент
                        if (i > 0)
                            localDirectiveList += ",\n";

                        // Если мы дошли до сюда, то можем сбросить результат в общую переменную
                        localDirectiveList += varText;

                        // А так же запомнить индекс переменной и тип
                        varsIndex.Add(id.Item1, new IndexType(i++, id.Item2));
                    }
                    catch (KeyNotFoundException)
                    {
                        // Если мы попали сюда, что все очень плохо. Такого типа переменной не существует или же мы наткнулись на None
                        //  Поидее мы дожны сгенерировать исключение, но нам и этого достаточно
                        Console.WriteLine("Отсутствует тип " + id.Item2.ToString() + " для переменной " + id.Item1.ToString());
                    }
                }
            }

            /// <summary>
            /// Перегруженная функция индексации
            /// </summary>
            /// <param name="IdName">имя</param>
            /// <returns>Tuple<int, CType> Индекс и тип</returns>
            public IndexType this[string IdName]
            {
                get
                {   
                    return (varsIndex.ContainsKey(IdName)) ? varsIndex[IdName]
                                                           : null;
                }
            }
        }

        /// <summary>
        /// Класс базового блока
        /// </summary>
        public class ILBLock
        {
            /// <summary>
            /// IL код блока
            /// </summary>
            private string ILcode = "";

            public ILBLock(BaseBlock block)
            {
                foreach (var codeLine in block.Code)
                {
                    if (comment3AdrCode)
                        ILcode += Environment.NewLine + "// " + codeLine.ToString() + Environment.NewLine;

                    ILcode += IL.code(codeLine);
                }
            }

            public override string ToString()
            {
                return ILcode + "\n";
            }
        }

        public static string text = "";

        private LinkedList<ILBLock> blocks = new LinkedList<ILBLock>();

        private void processBlocks(ControlFlowGraph CFG)
        {
            IL.Local = Local;
            var BlocksSelect = from block in CFG.GetBlocks()
                         where block.Code.Count > 0
                         select block;
            foreach (BaseBlock block in BlocksSelect)
            {
                blocks.AddLast(new ILBLock(block));
            }
        }

        public string code(ControlFlowGraph CFG)
        {
            processBlocks(CFG);

            text  = ".assembly extern mscorlib {} " + Environment.NewLine;
            text += ".assembly CompileReadyProgram {} " + Environment.NewLine;
            text += ".method static public void main() cil managed" + Environment.NewLine;
            text += "{" + Environment.NewLine;
            text += ".entrypoint" + Environment.NewLine;
            text += ".maxstack " + IL.MaxStackSize.ToString() + Environment.NewLine;
            text += Local.generateDerective();

            foreach (ILBLock block in blocks)
            {
                text += block.ToString();
            }

            text += "ret" + Environment.NewLine;
            text += "}";

            return text;
        }

        string CodeGenerator.code(ControlFlowGraph CFG)
        {
            return ((ILAsm)this).code(CFG);
        }

        public void compileExe(ControlFlowGraph CFG, string outputDir)
        {
            string outFileNameIL = outputDir +
                Path.GetFileNameWithoutExtension(SimpleCompiler.SimpleCompilerMain.FileName)+".il";
            string outFileNameEXE = outputDir +
                Path.GetFileNameWithoutExtension(SimpleCompiler.SimpleCompilerMain.FileName)+".exe";
            File.WriteAllText(outFileNameIL, code(CFG));    
            Process.Start("ilasm.exe", outFileNameIL + " /exe");
        }
    }

    
}


