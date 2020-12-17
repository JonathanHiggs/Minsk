namespace Minsk.CodeAnalysis.Text
{
    // ToDo: Maybe a struct?
    public sealed class TextLine
    {
        public TextLine(SourceText source, int start, int length, int lengthIncludingLineBreak)
        {
            Source = source;
            Start = start;
            Length = length;
            LengthIncludingLineBreak = lengthIncludingLineBreak;
        }

        public SourceText Source { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LengthIncludingLineBreak { get; }

        public TextSpan Span => new TextSpan(Start, Length);
        public TextSpan SpanIncludingLineBreak => new TextSpan(Start, LengthIncludingLineBreak);

        public string Substring(int start) => Source.ToString(Start + start, End - Start - start);
        public string Substring(int start, int length) => Source.ToString(Start + start, length);
        public override string ToString() => Source.ToString(Span);

    }
}
