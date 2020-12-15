using System.Collections.Generic;
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

        public EvaluationResult Evaluate(Dictionary<string, object> variables)
        {
            var diagnostics = Syntax.Diagnostics;
            var binder = new Binder(diagnostics, variables);
            var boundExpression = binder.BindExpression(Syntax.Root);

            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundExpression, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(diagnostics, value);
        }
    }
}