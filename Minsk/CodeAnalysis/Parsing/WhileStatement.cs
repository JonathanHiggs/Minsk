using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class WhileStatement : Statement
    {
        public WhileStatement(LexToken whileKeyword, Expression condition, Statement body)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            Body = body;
        }

        public LexToken WhileKeyword { get; }
        public Expression Condition { get; }
        public Statement Body { get; }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return Condition;
                yield return Body;
            }
        }

        public override LexToken FirstToken => WhileKeyword;

        public override LexToken LastToken => Body.LastToken;
    }
}