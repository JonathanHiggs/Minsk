using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class CallExpression : Expression
    {
        public CallExpression(
            SyntaxTree syntaxTree,
            LexToken identifier,
            LexToken openParens,
            SeparatedSyntaxList<Expression> arguments,
            LexToken closeParens
        )
            : base(syntaxTree)
        {
            Identifier = identifier;
            OpenParens = openParens;
            Arguments = arguments;
            CloseParens = closeParens;

            foreach (var argument in Arguments)
                argument.Parent = this;
        }

        public LexToken Identifier { get; }
        public LexToken OpenParens { get; }
        public SeparatedSyntaxList<Expression> Arguments { get; }
        public LexToken CloseParens { get; }

        public override string Text => Identifier.Text;

        public override LexToken FirstToken => Identifier;

        public override LexToken LastToken => CloseParens;

        public override IEnumerable<SyntaxNode> Children => Arguments;

        public override SyntaxKind Kind => SyntaxKind.CallExpression;
    }
}