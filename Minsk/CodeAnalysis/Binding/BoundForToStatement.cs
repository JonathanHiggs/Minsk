using System.Collections.Generic;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundForToStatement : BoundLoopStatement
    {
        public BoundForToStatement(
            VariableSymbol variable,
            BoundExpression lowerBound,
            BoundExpression upperBound,
            BoundStatement body,
            BoundLabel breakLabel,
            BoundLabel continueLabel)
            : base(body, breakLabel, continueLabel)
        {
            Variable = variable;
            LowerBound = lowerBound;
            UpperBound = upperBound;

            LowerBound.Parent = this;
            UpperBound.Parent = this;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ForToStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return LowerBound;
                // yield return ContinueLabel;
                yield return UpperBound;
                yield return Body;
                // yield return BreakLabel;
            }
        }

        protected override string PrettyPrintText()
            => $"{Variable.Name}, {Variable.Type.Name}";
    }
}