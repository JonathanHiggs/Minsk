using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Text
{
    public sealed class SourceText
    {
        private readonly string text;

        public SourceText(string text, string fileName)
        {
            this.text = text;
            FileName = fileName;
            Lines = ParseLines(this, text);
        }

        public static SourceText From(string text, string fileName = "")
            => new SourceText(text, fileName);

        public string FileName { get; }

        public ImmutableArray<TextLine> Lines { get; }

        public char this[int index] => text[index];

        public int Length => text.Length;

        public int LineIndexOf(int position)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Start;

                if (position == start)
                    return index;

                if (start > position)
                    upper = index - 1;
                else
                    lower = index + 1;
            }

            return lower - 1;
        }

        public override string ToString() => text;

        public string ToString(int start, int length) => text.Substring(start, length);

        public string ToString(TextSpan span) => text.Substring(span.Start, span.Length);

        #region Parse Lines

        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();

            var position = 0;
            var lineStart = 0;

            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                    position++;
                else
                {
                    AddLine(result, sourceText, position, lineStart, lineBreakWidth);
                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (position >= lineStart)
                AddLine(result, sourceText, position, lineStart, 0);

            return result.ToImmutable();
        }

        private static void AddLine(
            ImmutableArray<TextLine>.Builder builder,
            SourceText sourceText,
            int position,
            int lineStart,
            int lineBreakWidth)
        {
            var length = position - lineStart;
            var includingBreak = length + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, length, includingBreak);
            builder.Add(line);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            var c = text[position];
            var l = position + 1 >= text.Length ? '\0' : text[position + 1];

            return (c, l) switch
            {
                ('\r', '\n') => 2,
                ('\r', '\r') => 1,
                _ => 0
            };
        }

        #endregion Parse Lines

        internal TextLocation Span(int start, int length)
            => new TextLocation(this, new TextSpan(start, length));
    }
}