using System;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    internal static class SyntaxFacts
    {
        internal static int UnaryOperatorPrecedence(this TokenType type)
        {
            switch (type)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                    return 3;   // Bind higher than the binary operators

                default:
                    return 0;
            }
        }

        internal static int BinaryOperatorPrecedence(this TokenType type)
        {
            switch (type)
            {
                case TokenType.Star:
                case TokenType.ForwardSlash:
                    return 2;

                case TokenType.Plus:
                case TokenType.Minus:
                    return 1;

                default:
                    return 0;
            }
        }

        internal static TokenType KeywordKind(string tokenText)
        {
            return tokenText switch {
                "true" => TokenType.TrueKeyword,
                "false" => TokenType.FalseKeyword,

                _ => TokenType.Identifier
            };
        }
    }
}