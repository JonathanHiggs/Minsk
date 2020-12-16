using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    internal static class SyntaxFacts
    {
        internal static int UnaryOperatorPrecedence(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Bang:
                    return 6;

                default:
                    return 0;
            }
        }

        internal static int BinaryOperatorPrecedence(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Star:
                case TokenKind.ForwardSlash:
                    return 5;

                case TokenKind.Plus:
                case TokenKind.Minus:
                    return 4;

                case TokenKind.EqualsEquals:
                case TokenKind.BangEquals:
                    return 3;

                case TokenKind.AmpersandAmperand:
                    return 2;

                case TokenKind.PipePipe:
                    return 1;

                default:
                    return 0;
            }
        }

        internal static TokenKind KeywordKind(string tokenText)
            => tokenText switch {
                "true"  => TokenKind.TrueKeyword,
                "false" => TokenKind.FalseKeyword,

                _ => TokenKind.Identifier
            };

        internal static bool IsKeyword(this TokenKind kind)
            => kind switch {
                TokenKind.TrueKeyword   => true,
                TokenKind.FalseKeyword  => true,

                _ => false
            };

        internal static bool RequiresSeperator(this TokenKind kind1, TokenKind kind2)
            => (kind1, kind2) switch {
                (TokenKind.Identifier,      TokenKind.Identifier)       => true,

                (TokenKind.Bang,            TokenKind.Equals)           => true,
                (TokenKind.BangEquals,      TokenKind.Equals)           => true,
                (TokenKind.Bang,            TokenKind.EqualsEquals)     => true,

                (TokenKind.Equals,          TokenKind.Equals)           => true,
                (TokenKind.Equals,          TokenKind.EqualsEquals)     => true,
                (TokenKind.EqualsEquals,    TokenKind.Equals)           => true,

                (TokenKind.Number,          TokenKind.Number)           => true,

                (TokenKind k1, TokenKind k2) when k1.IsKeyword() && k2.IsKeyword()  => true,
                (TokenKind k1, TokenKind.Identifier) when k1.IsKeyword()            => true,
                (TokenKind.Identifier, TokenKind k2) when k2.IsKeyword()            => true,

                _ => false
            };
    }
}