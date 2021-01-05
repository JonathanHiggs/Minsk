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
        private BoundProgram program;

        public Compilation(SyntaxTree tree)
            : this(tree.Diagnostics, tree)
        { }

        public Compilation(DiagnosticBag diagnostics, params SyntaxTree[] syntaxTrees)
            : this(diagnostics, null, syntaxTrees)
        { }

        private Compilation(DiagnosticBag diagnostics, Compilation previous, SyntaxTree[] syntaxTrees)
        {
            Diagnostics = diagnostics;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation CreateScript(SyntaxTree syntaxTree)
            => new Compilation(syntaxTree?.Diagnostics ?? new DiagnosticBag(), null, syntaxTree);

        public static Compilation CreateScript(Compilation previous, SyntaxTree syntaxTree)
            => new Compilation(syntaxTree.Diagnostics, previous, new SyntaxTree[] { syntaxTree });

        public static Compilation Compile(SyntaxTree syntaxTree)
            => new Compilation(syntaxTree.Diagnostics, null, syntaxTree);

        public static Compilation Compile(DiagnosticBag diagnostics, IEnumerable<SyntaxTree> syntaxTrees)
            => new Compilation(diagnostics, syntaxTrees.ToArray());

        public DiagnosticBag Diagnostics { get; }

        public Compilation Previous { get; }

        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope is null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees, Diagnostics);
                    Interlocked.CompareExchange(ref this.globalScope, globalScope, null);
                }
                return globalScope;
            }
        }

        internal BoundProgram Program
        {
            get
            {
                if (program is null)
                {
                    var program = Binder.BindProgram(Previous?.Program, GlobalScope, Diagnostics);
                    Interlocked.CompareExchange(ref this.program, program, null);
                }
                return program;
            }
        }


        public IEnumerable<Symbol> Symbols
            => GlobalScope.Functions.Cast<Symbol>()
                .Concat(GlobalScope.Variables)
                .Concat(Program.AllFunctions().Select(f => f.Symbol));

        public Compilation ContinueWith(SyntaxTree syntaxTree)
            => new Compilation(syntaxTree.Diagnostics, this, new SyntaxTree[] { syntaxTree });

        public EvaluationResult Evaluate()
            => Evaluate(new Dictionary<VariableSymbol, object>());

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            // Side effects on a property access... yay!
            var diagnostics = GlobalScope.Diagnostics;
            if (diagnostics.Any())
                return new EvaluationResult(null, diagnostics.ToImmutableArray());

            // Side effects on a property access... yay x2!
            var program = Program;

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
            var program = Program;
            var tree = program.Functions[function];

            function.WriteTo(writer);
            writer.WriteLine();
            tree.WriteTo(writer);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Program;

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