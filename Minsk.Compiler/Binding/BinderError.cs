using System;

using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Binding
{
    public sealed class BinderError : CompilerError
    {
        public BinderError(SyntaxNode node, string message)
            : base(message)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public SyntaxNode Node { get; }
        
        public override string ToString()
            => $"BindingError  "
             + $"{Node.FirstToken.Position}-{Node.LastToken.Position + Node.LastToken.Text.Length}  "
             + $"\"{Node.LongText}\"  {Message}";
    }
}