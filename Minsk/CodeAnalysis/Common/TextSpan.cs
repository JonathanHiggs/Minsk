namespace Minsk.CodeAnalysis.Common
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;

        public TextSpan Advance()
            => new TextSpan(Start + Length, 0);

        public TextSpan Inc() => Inc(1);

        public TextSpan Inc(int value)
            => new TextSpan(Start, Length + value);
    }
}