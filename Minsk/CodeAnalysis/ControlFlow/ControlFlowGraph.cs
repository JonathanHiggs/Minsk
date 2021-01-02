using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        public ControlFlowGraph(
            AtomicBlock start,
            AtomicBlock end,
            IEnumerable<AtomicBlock> blocks,
            IEnumerable<Branch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks.ToImmutableArray();
            Branches = branches.ToImmutableArray();
        }

        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            var blocks = AtomicBlockBuilder.Build(body);
            var graph = ControlFlowGraphBuilder.Build(blocks);
            return graph;
        }

        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            var graph = Create(body);

            if (!graph.End.Incoming.Any())
                return false;

            foreach (var branch in graph.End.Incoming)
            {
                if (!branch.From.Statements.Any())
                    return false;

                var last = branch.From.Statements.Last();
                if (last.Kind != BoundNodeKind.ReturnStatement)
                    return false;
            }

            return true;
        }

        public AtomicBlock Start { get; }
        public AtomicBlock End { get; }
        public ImmutableArray<AtomicBlock> Blocks { get; }
        public ImmutableArray<Branch> Branches { get; }

        public void WriteTo(TextWriter writer)
        {
            static string Quote(string text)
            {
                return "\"" + text.Replace("\"", "\\\"") + "\"";
            }

            writer.WriteLine("digraph G {");

            var blockIds = new Dictionary<AtomicBlock, string>();

            for (var i = 0; i < Blocks.Length; i++)
            {
                var id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (var block in Blocks)
            {
                var id = blockIds[block];
                var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
                writer.WriteLine($"    {id} [label = {label}, shape = box]");
            }

            foreach (var branch in Branches)
            {
                var fromId = blockIds[branch.From];
                var toId = blockIds[branch.To];
                var label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }
    }
}
