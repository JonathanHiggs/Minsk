using System;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
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
        {
            return tokenText switch {
                "true" => TokenKind.TrueKeyword,
                "false" => TokenKind.FalseKeyword,

                _ => TokenKind.Identifier
            };
        }
    }
}