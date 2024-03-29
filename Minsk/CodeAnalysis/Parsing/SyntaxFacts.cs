using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public static class SyntaxFacts
    {
        public static bool IsUnaryOperator(this TokenKind kind)
            => kind.UnaryOperatorPrecedence() > 0;

        public static int UnaryOperatorPrecedence(this TokenKind kind)
        {
            // ToDo: Add UnaryOperator attributes to TokeKind and DebugAssert false cases
            switch (kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Bang:
                case TokenKind.Tilde:
                case TokenKind.Hat:
                    return 6;

                default:
                    return 0;
            }
        }

        public static bool IsBinaryOperator(this TokenKind kind)
            => kind.BinaryOperatorPrecedence() > 0;

        public static int BinaryOperatorPrecedence(this TokenKind kind)
        {
            // ToDo: Add BinaryOperator attribute to TokenKind and Debug.Assert false cases
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
                case TokenKind.Less:
                case TokenKind.LessOrEquals:
                case TokenKind.Greater:
                case TokenKind.GreaterOrEquals:
                    return 3;

                case TokenKind.Ampersand:
                case TokenKind.AmpersandAmpersand:
                    return 2;

                case TokenKind.Pipe:
                case TokenKind.PipePipe:
                case TokenKind.Hat:
                    return 1;

                default:
                    return 0;
            }
        }

        public static TokenKind KeywordOrIdentifierKind(string tokenText)
            => tokenText switch {
                // ToDo: Add Keyword attributes to TokenKind and Debug.Assert false cases
                "break"     => TokenKind.BreakKeyword,
                "continue"  => TokenKind.ContinueKeyword,
                "else"      => TokenKind.ElseKeyword,
                "false"     => TokenKind.FalseKeyword,
                "for"       => TokenKind.ForKeyword,
                "function"  => TokenKind.FunctionKeyword,
                "if"        => TokenKind.IfKeyword,
                "let"       => TokenKind.LetKeyword,
                "return"    => TokenKind.ReturnKeyword,
                "to"        => TokenKind.ToKeyword,
                "true"      => TokenKind.TrueKeyword,
                "var"       => TokenKind.VarKeyword,
                "while"     => TokenKind.WhileKeyword,

                _           => TokenKind.Identifier
            };

        public static bool IsKeyword(this TokenKind kind)
            => kind switch {
                TokenKind.BreakKeyword      => true,
                TokenKind.ContinueKeyword   => true,
                TokenKind.ElseKeyword       => true,
                TokenKind.FalseKeyword      => true,
                TokenKind.ForKeyword        => true,
                TokenKind.FunctionKeyword   => true,
                TokenKind.IfKeyword         => true,
                TokenKind.ReturnKeyword     => true,
                TokenKind.LetKeyword        => true,
                TokenKind.ToKeyword         => true,
                TokenKind.TrueKeyword       => true,
                TokenKind.VarKeyword        => true,
                TokenKind.WhileKeyword      => true,

                _ => false
            };

        public static bool RequiresSeperator(this (TokenKind First, TokenKind Second) pair)
            => pair switch {
                (TokenKind.Identifier,      TokenKind.Identifier)       => true,

                (TokenKind.Bang,            TokenKind.Equals)           => true,
                (TokenKind.BangEquals,      TokenKind.Equals)           => true,
                (TokenKind.Bang,            TokenKind.EqualsEquals)     => true,

                (TokenKind.Equals,          TokenKind.Equals)           => true,
                (TokenKind.Equals,          TokenKind.EqualsEquals)     => true,
                (TokenKind.Equals,          TokenKind.Less)             => true,
                (TokenKind.Equals,          TokenKind.Greater)          => true,
                (TokenKind.EqualsEquals,    TokenKind.Equals)           => true,

                (TokenKind.Less,            TokenKind.Equals)           => true,
                (TokenKind.Less,            TokenKind.EqualsEquals)     => true,
                (TokenKind.LessOrEquals,    TokenKind.Equals)           => true,
                (TokenKind.Greater,         TokenKind.Equals)           => true,
                (TokenKind.Greater,         TokenKind.EqualsEquals)     => true,
                (TokenKind.GreaterOrEquals, TokenKind.Equals)           => true,

                (TokenKind.Number,          TokenKind.Number)           => true,
                (TokenKind.String,          TokenKind.String)           => true,

                (TokenKind k1, TokenKind k2) when k1.IsKeyword() && k2.IsKeyword()  => true,
                (TokenKind k1, TokenKind.Identifier) when k1.IsKeyword()            => true,
                (TokenKind.Identifier, TokenKind k2) when k2.IsKeyword()            => true,

                _ => false
            };
    }
}