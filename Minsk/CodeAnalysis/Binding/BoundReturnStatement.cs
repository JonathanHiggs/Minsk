using System;
using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; }

        public override IEnumerable<BoundNode> Children
        {  get { yield return Expression; } }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

        protected override string PrettyPrintText()
            => string.Empty;
    }
}
