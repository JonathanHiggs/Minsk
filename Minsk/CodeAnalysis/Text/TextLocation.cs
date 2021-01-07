namespace Minsk.CodeAnalysis.Text
{
    public struct TextLocation
    {
        public TextLocation(SourceText source, TextSpan span)
        {
            Source = source;
            Span = span;
        }


        public static TextLocation Empty => new TextLocation(SourceText.Empty, TextSpan.From(0, 0));


        public SourceText Source { get; }
        public TextSpan Span { get; }

        public string FileName => Source.FileName;
        public int StartLine => Source.LineIndexOf(Span.Start);
        public int StartCharacter => Span.Start - Source.Lines[StartLine].Start;
        public int EndLine => Source.LineIndexOf(Span.End);
        public int EndCharacter => Span.End - Source.Lines[EndLine].Start;
    }
}
