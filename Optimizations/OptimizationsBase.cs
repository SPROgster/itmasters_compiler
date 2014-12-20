using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.Analysis;
using SimpleLang.MiddleEnd;

namespace SimpleLang.Optimizations
{
    public interface LocalOptimization
    {
        /// <summary>
        /// Выполняет оптимизацию блока
        /// </summary>
        /// <param name="block"> Оптимизируемый блок </param>
        /// <returns> Были ли внесены какие-то изменения </returns>
        bool Optimize(BaseBlock block);

        //Функция возвращает нормально воспринимаемое человеком название оптимизации
        string GetName();
    }

    public interface GlobalOptimization
    {
        /// <summary>
        /// Выполняет межблочную оптимизацию
        /// </summary>
        /// <param name="block"> ГГраф оптимизируемой программы </param>
        /// <returns> Были ли внесены какие-то изменения </returns>
        bool Optimize(ControlFlowGraph CFG);

        //Функция возвращает нормально воспринимаемое человеком название оптимизации
        string GetName();
    }
}