using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexToken
    {
        public LexToken(TokenKind tokenKind, int position, int length, string text, object value = null)
            : this(tokenKind, new TextSpan(position, length), text, value)
        { }

        public LexToken(TokenKind tokenKind, TextSpan span, string text, object value = null)
        {
            Kind = tokenKind;
            Span = span;
            Text = text;
            Value = value;
        }

        public TokenKind Kind { get; }
        public TextSpan Span { get; }
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