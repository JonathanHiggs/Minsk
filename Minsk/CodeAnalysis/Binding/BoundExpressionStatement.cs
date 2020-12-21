using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
            Expression.Parent = this;
        }

        public BoundExpression Expression { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
            }
        }

        protected override string PrettyPrintText()
            => string.Empty;
    }
}