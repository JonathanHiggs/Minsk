using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lowering;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope globalScope;

        public Compilation(SyntaxTree syntaxTree)
            : this(null, syntaxTree)
        { }

        private Compilation(Compilation previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public static Compilation CreateScript(SyntaxTree syntaxTree)
            => new Compilation(null, syntaxTree);

        public static Compilation CreateScript(Compilation previous, SyntaxTree syntaxTree)
            => new Compilation(previous, syntaxTree);

        public Compilation Previous { get; }

        public SyntaxTree SyntaxTree { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope is null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root, SyntaxTree.Diagnostics);
                    Interlocked.CompareExchange(ref this.globalScope, globalScope, null);
                }
                return globalScope;
            }
        }

        public IEnumerable<Symbol> Symbols
            => Enumerable.Concat<Symbol>(GlobalScope.Functions, GlobalScope.Variables);

        public Compilation ContinueWith(SyntaxTree syntaxTree)
            => new Compilation(this, syntaxTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = GlobalScope.Diagnostics;
            if (diagnostics.Any())
                return new EvaluationResult(null, diagnostics.ToImmutableArray());

            var program = Binder.BindProgram(GlobalScope, diagnostics);

            EmitControlFlowGraph(program);

            if (program.Diagnostics.Any())
                return new EvaluationResult(null, diagnostics.ToImmutableArray());

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(value, ImmutableArray<Diagnostic>.Empty);
        }

        internal void EmitControlFlowGraph(BoundProgram program)
        {
            var cfgStatement = program.Statement.Statements.Any()
                ? program.Statement
                : program.Functions.Any()
                    ? program.Functions.Last().Value
                    : null;

            var cfg = ControlFlowGraph.Create(cfgStatement);

            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            using var streamWriter = new StreamWriter(cfgPath);

            cfg.WriteTo(streamWriter);
        }

        public void EmitTree(FunctionSymbol function, TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope, new DiagnosticBag());
            var tree = program.Functions[function];

            function.WriteTo(writer);
            writer.WriteLine();
            tree.WriteTo(writer);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope, new DiagnosticBag());

            if (program.Statement.Statements.Any())
            {
                program.WriteTo(writer);
            }
            else
            {
                foreach (var function in program.Functions)
                {
                    if (!GlobalScope.Functions.Contains(function.Key))
                        continue;

                    function.Key.WriteTo(writer);
                    writer.WriteLine();
                    function.Value.WriteTo(writer);
                }
            }
        }
    }
}