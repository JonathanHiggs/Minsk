using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Emit;
using Minsk.CodeAnalysis.Lowering;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope globalScope;
        private BoundProgram program;

        private Compilation(
            DiagnosticBag diagnostics,
            IEnumerable<SyntaxTree> syntaxTrees,
            Compilation previous = null,
            bool isScript = false)
        {
            Diagnostics = diagnostics;
            Previous = previous;
            IsScript = isScript;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(SyntaxTree syntaxTree)
            => new Compilation(syntaxTree.Diagnostics, new[] { syntaxTree }, previous: null, isScript: false);

        public static Compilation Create(DiagnosticBag diagnostics, IEnumerable<SyntaxTree> syntaxTrees)
            => new Compilation(diagnostics, syntaxTrees, previous: null, isScript: false);

        public static Compilation Empty
            => new Compilation(new DiagnosticBag(), Enumerable.Empty<SyntaxTree>(), previous: null, isScript: false);

        public static Compilation CreateScript(SyntaxTree syntaxTree)
            => new Compilation(syntaxTree.Diagnostics, new[] { syntaxTree }, previous: null, isScript: true);

        public static Compilation CreateScript(SyntaxTree syntaxTree, Compilation previous)
            => new Compilation(syntaxTree.Diagnostics, new[] { syntaxTree }, previous: previous, isScript: true);

        public DiagnosticBag Diagnostics { get; }

        public Compilation Previous { get; }

        public bool IsScript { get; }

        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

        public FunctionSymbol MainFunction => GlobalScope.MainFunction;

        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;

        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;


        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope is null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees, Diagnostics);
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
                    var program = Binder.BindProgram(IsScript, Previous?.Program, GlobalScope, Diagnostics);
                    Interlocked.CompareExchange(ref this.program, program, null);
                }
                return program;
            }
        }


        public IEnumerable<Symbol> Symbols
        {
            get
            {
                var submission = this;
                var seenSymbolNames = new HashSet<string>();

                foreach (var builtin in BuiltinFunctions.All)
                    yield return builtin;

                while (submission is not null)
                {
                    foreach (var function in submission.Functions)
                        if (seenSymbolNames.Add(function.Name))
                            yield return function;

                    foreach (var variable in submission.Variables)
                        if (seenSymbolNames.Add(variable.Name))
                            yield return variable;

                    submission = submission.Previous;
                }
            }
        }

        public EvaluationResult Evaluate()
            => Evaluate(new Dictionary<VariableSymbol, object>());

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {;
            if (GlobalScope.Diagnostics.Any())
                return new EvaluationResult(null, GlobalScope.Diagnostics.ToImmutableArray());

            if (Program.Diagnostics.Any())
                return new EvaluationResult(null, Program.Diagnostics.ToImmutableArray());

            var evaluator = new Evaluator(Program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(value, ImmutableArray<Diagnostic>.Empty);
        }

        public EmitResult Emit(string moduleName, IEnumerable<string> references, string outputPath)
        {
            return Emitter.Emit(Program, moduleName, references, outputPath);
        }

        public void EmitControlFlowGraph(FunctionSymbol function)
        {
            var cfgStatement =
                program.Functions.SingleOrDefault(kvp => kvp.Key.Name == function.Name).Value;

            var cfg = ControlFlowGraph.Create(cfgStatement);

            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            using var streamWriter = new StreamWriter(cfgPath);

            cfg.WriteTo(streamWriter);
        }

        public void EmitTree(FunctionSymbol function, TextWriter writer)
        {
            var tree = Program.Functions[function];

            function.WriteTo(writer);
            writer.WriteLine();
            tree.WriteTo(writer);
        }

        public void EmitTree(TextWriter writer)
        {
            foreach (var function in Program.Functions)
            {
                function.Key.WriteTo(writer);
                writer.WriteLine();
                function.Value.WriteTo(writer);
                writer.WriteLine();
            }
        }
    }
}