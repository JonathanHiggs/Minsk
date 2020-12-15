using System;

using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexError : Diagnostic
    {
        public LexError(int position, int length, string text, string message)
            : base(position, length, message)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text { get; }

        public override string ToString()
            => $"LexingError  {Source.Start}  \"{Text}\"  {Message}";
    }
}