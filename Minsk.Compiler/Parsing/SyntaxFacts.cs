using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    internal static class SyntaxFacts
    {
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
    }
}