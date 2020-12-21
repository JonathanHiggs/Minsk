using System.Collections.Generic;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundForToStatement : BoundStatement
    {
        public BoundForToStatement(
            VariableSymbol variable,
            BoundExpression lowerBound,
            BoundExpression upperBound,
            BoundStatement body)
        {
            Variable = variable;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Body = body;

            LowerBound.Parent = this;
            UpperBound.Parent = this;
            Body.Parent = this;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ForToStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return LowerBound;
                yield return UpperBound;
                yield return Body;
            }
        }

        protected override string PrettyPrintText()
            => $"{Variable.Name}, {Variable.Type.Name}";
    }
}