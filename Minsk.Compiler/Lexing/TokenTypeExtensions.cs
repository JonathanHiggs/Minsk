namespace Minsk.Compiler.Lexing
{
    public static class TokenTypeExtensions
    {
        public static bool IsOperator(this TokenType type)
        {
            switch (type)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Star:
                case TokenType.ForwardSlash:
                    return true;

                default:
                    return false;
            }
        }
    }
}