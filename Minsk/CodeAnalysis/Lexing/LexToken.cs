namespace Minsk.CodeAnalysis.Lexing
{
    public class LexToken
    {
        public LexToken(TokenKind tokenType, int position, string text, object value = null)
        {
            Kind = tokenType;
            Position = position;
            Text = text;
            Value = value;
        }

        public TokenKind Kind { get; }
        public int Position { get;}
        public string Text { get; }
        public object Value { get; }


        public override string ToString()
        {
            switch (Kind)
            {
                case TokenKind.Number:
                case TokenKind.String:
                    return $"{Kind,-20}{$"\"{Text}\"",-20}{Value}";

                case TokenKind.Unknown:
                    return $"{Kind,-20}\"{Text}\"";

                default:
                    return $"{Kind,-20}\"{Text}\"";
            }
        }
    }
}