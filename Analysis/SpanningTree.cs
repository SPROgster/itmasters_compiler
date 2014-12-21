using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleLang.MiddleEnd;

namespace SimpleLang.Analysis
{
    // класс остовного дерева для графа потока управления
    public class SpanningTree
    {
        ControlFlowGraph gr;
        public HashSet<BaseBlock> spTree;
        // ребро дерева
        public class Edge
        {
            // характеризуется
            int src;    // узлом-началом
            int dst;    // узлом-концом

            public Edge(int s, int d)
            {
                this.src = s;
                this.dst = d;
            }

            public int Source
            {
                get { return this.src; }
            }

            public int Destination
            {
                get { return this.dst; }
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", this.src, this.dst);
            }
        };
        //
        HashSet<Edge> edges;
        //
        int blockCounter;
        //
        public SpanningTree(ControlFlowGraph gr)
        {
            this.gr = gr;
            this.spTree = new HashSet<BaseBlock>();
            this.edges = new HashSet<Edge>();
            this.blockCounter = 0;
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
            int old_nBlock = u.nBlock;
            u.nBlock = blockCounter++;
            foreach (var child in this.gr.GetOutputs(u).Where(bl => !this.spTree.Contains(bl) && bl != this.gr.GetStart() && bl != this.gr.GetEnd()))
            {
                this.edges.Add(new Edge(old_nBlock, child.nBlock));
                DFS_Visit(child);
            }
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
        // вывод ребер на экран
        public void PrintEdges()
        {
            foreach (var e in this.edges)
                Console.WriteLine(e);
        }
        //
        public ControlFlowGraph Graph
        {
            get { return this.gr; }
        }
        // возвращает набор ребер
        public HashSet<Edge> Edges()
        {
            return this.edges;
        }
    }
}
