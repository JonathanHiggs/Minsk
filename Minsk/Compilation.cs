using System.Linq;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis
{
    public sealed class Compilation
    {
        public Compilation(SyntaxTree syntax)
        {
            Syntax = syntax;
        }

        public SyntaxTree Syntax { get; }

        public EvaluationResult Evaluate()
        {
            var diagnostics = Syntax.Diagnostics;
            var binder = new Binder(diagnostics);
            var boundExpression = binder.BindExpression(Syntax.Root);

            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundExpression);
            var value = evaluator.Evaluate();

            return new EvaluationResult(diagnostics, value);
        }
    }
}