using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    // ToDo: move to Text namespace
    // ToDo: rename TextCursor
    // ToDo: add ref to SourceText and ensure this.End <= source.End
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

        public void Advance(int numberOfChars = 1)
        {
            Length += numberOfChars;
        }

        public TextSpan Consume(int numberOfChars = 0)
        {
            var span = new TextSpan(Start, Length + numberOfChars);
            Start = Start + Length + numberOfChars;
            Length = 0;
            return span;
        }
    }
}