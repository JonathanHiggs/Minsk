namespace Minsk.Compiler.Lexing
{
    public class SyntaxToken
    {
        public SyntaxToken(TokenType tokenType, int position, string text, object value = null)
        {
            TokenType = tokenType;
            Position = position;
            Text = text;
            Value = value;
        }

        public TokenType TokenType { get; }
        public int Position { get;} 
        public string Text { get; }
        public object Value { get; }


        public override string ToString()
        {
            switch (TokenType)
            {
                case TokenType.Number:
                case TokenType.String:
                    return $"{TokenType,-20}{$"\"{Text}\"",-20}{Value}";

                case TokenType.Unknown:
                    return $"{TokenType,-20}\"{Text}\"";

                default:
                    return $"{TokenType,-20}\"{Text}\"";
            }
        }
    }
}