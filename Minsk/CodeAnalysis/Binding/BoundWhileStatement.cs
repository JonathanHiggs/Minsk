using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement(
            BoundExpression condition,
            BoundStatement body,
            BoundLabel breakLabel,
            BoundLabel continueLabel)
            : base(body, breakLabel, continueLabel)
        {
            Condition = condition;
            Condition.Parent = this;
        }

        public BoundExpression Condition { get; }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                //yield return ContinueLabel;
                yield return Condition;
                yield return Body;
                //yield return BreakLabel;
            }
        }

        protected override string PrettyPrintText()
            => string.Empty;
    }
}