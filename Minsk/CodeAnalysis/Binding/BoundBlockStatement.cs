using System.Collections.Generic;
using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
        {
            Statements = statements;

            foreach (var statement in Statements)
                statement.Parent = this;
        }

        public ImmutableArray<BoundStatement> Statements { get; }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (var child in Statements)
                    yield return child;
            }
        }

        protected override string PrettyPrintText()
            => string.Empty;
    }
}