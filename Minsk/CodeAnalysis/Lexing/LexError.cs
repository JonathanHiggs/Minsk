using System;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexError : Diagnostic
    {
        public LexError(TextSpan span, string text, string message)
            : base(span, message)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text { get; }

        public override string ToString()
            => $"LexingError  {Source.Start}  \"{Text}\"  {Message}";
    }
}