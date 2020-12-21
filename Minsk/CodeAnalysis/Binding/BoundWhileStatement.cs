using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body)
        {
            Condition = condition;
            Body = body;

            Condition.Parent = this;
            Body.Parent = this;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Condition;
                yield return Body;
            }
        }

        protected override string PrettyPrintText()
            => string.Empty;
    }
}