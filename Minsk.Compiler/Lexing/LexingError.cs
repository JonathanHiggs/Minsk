using System;

using Minsk.Compiler.Core;

namespace Minsk.Compiler.Lexing
{
    public class LexingError : CompilerError
    {
        public LexingError(int position, int length, string text, string message)
        {
            Position = position;
            Length = length;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public int Position { get; }
        public int Length { get; }
        public string Text { get; }
        public override string Message { get; }


        public override string ToString()
            => $"LexingError: {Position}:{Text}    {Message}";
    }
}