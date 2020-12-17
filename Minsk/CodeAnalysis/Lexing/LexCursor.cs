using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    internal sealed class LexCursor
    {
        public LexCursor()
        {
            Start = 0;
            Length = 0;
        }

        public int Start { get; private set; }
        public int Length { get; private set; }
        public int End => Start + Length;

        public static implicit operator TextSpan(LexCursor cursor)
            => new TextSpan(cursor.Start, cursor.Length);

        public void Advance(int value = 1)
        {
            Length += value;
        }

        public TextSpan Consume(int value = 0)
        {
            var span = new TextSpan(Start, Length + value);
            Start = Start + Length + value;
            Length = 0;
            return span;
        }
    }
}