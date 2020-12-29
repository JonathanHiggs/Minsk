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

        public Compilation ContinueWith(SyntaxTree syntaxTree)
            => new Compilation(this, syntaxTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = GlobalScope.Diagnostics;
            if (diagnostics.Any())
                return new EvaluationResult(null, diagnostics.ToImmutableArray());

            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(value, ImmutableArray<Diagnostic>.Empty);
        }

        public void EmitTree(TextWriter writer)
        {
            var statement = GetStatement();
            statement.PrettyPrint(writer);
        }

        private BoundBlockStatement GetStatement()
        {
            var result = GlobalScope.Statement;
            return Lowerer.Lower(result);
        }
    }
}