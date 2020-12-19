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
        }

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement ElseStatement { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalStatement;
    }
}