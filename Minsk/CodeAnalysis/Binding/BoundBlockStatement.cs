using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> boundStatements)
        {
            this.BoundStatements = boundStatements;
        }

        public ImmutableArray<BoundStatement> BoundStatements { get; }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    }
}