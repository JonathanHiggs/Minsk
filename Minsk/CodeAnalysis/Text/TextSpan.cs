using System;

namespace Minsk.CodeAnalysis.Text
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public static TextSpan From(int start, int length)
            => new TextSpan(start, length);

        public static TextSpan FromBounds(int start, int end)
            => new TextSpan(start, end - start);

        public TextSpan To(TextSpan span)
            => FromBounds(Math.Min(Start, span.Start), Math.Max(End, span.End));

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;

        public override string ToString() => $"{Start}..{End}";
    }
}