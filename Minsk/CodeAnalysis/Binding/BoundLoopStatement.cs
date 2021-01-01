namespace Minsk.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        public BoundLoopStatement(BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        {
            Body = body;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;

            Body.Parent = this;
        }

        public BoundStatement Body { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}