using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ConditionalStatement : Statement
    {
        public ConditionalStatement(
            SyntaxTree syntaxTree,
            LexToken ifToken,
            Expression condition,
            Statement thenStatement
        )
            : this(syntaxTree, ifToken, condition, thenStatement, null)
        { }

        public ConditionalStatement(
            SyntaxTree syntaxTree,
            LexToken ifToken,
            Expression condition,
            Statement thenStatement,
            ElseClauseSyntax elseNode
        )
            : base(syntaxTree)
        {
            IfKeyword = ifToken;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseClause = elseNode;

            Condition.Parent = this;
            ThenStatement.Parent = this;

            if (ElseClause is not null)
                ElseClause.Parent = this;
        }

        public LexToken IfKeyword { get; }
        public Expression Condition { get; }
        public Statement ThenStatement { get; }
        public ElseClauseSyntax ElseClause { get; }

        public override SyntaxKind Kind => SyntaxKind.ConditionalStatement;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return Condition;
                yield return ThenStatement;

                if (ElseClause is not null)
                    yield return ElseClause;
            }
        }

        public override LexToken FirstToken => IfKeyword;

        public override LexToken LastToken
            => ElseClause?.LastToken ?? ThenStatement.LastToken;
    }
}