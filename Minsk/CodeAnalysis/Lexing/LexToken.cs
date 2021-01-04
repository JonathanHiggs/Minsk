using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexToken
    {
        public LexToken(
            TokenKind tokenKind,
            TextLocation location,
            string text,
            object value = null,
            bool isMissing = false)
        {
            Kind = tokenKind;
            Location = location;
            Text = text;
            Value = value;
            IsMissing = isMissing;
        }

        public TokenKind Kind { get; }
        public TextLocation Location { get; }
        public string Text { get; }
        public object Value { get; }
        public bool IsMissing { get; }

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