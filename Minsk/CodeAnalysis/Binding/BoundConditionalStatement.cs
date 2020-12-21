using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalStatement : BoundStatement
    {
        public BoundConditionalStatement(
            BoundExpression condition,
            BoundStatement thenStatement,
            BoundStatement elseStatement)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;

            ThenStatement.Parent = this;

            if (ElseStatement is not null)
                ElseStatement.Parent = this;
        }

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement ElseStatement { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Condition;
                yield return ThenStatement;

                if (ElseStatement is not null)
                    yield return ElseStatement;
            }
        }

        protected override string PrettyPrintText()
            => string.Empty;
    }
}