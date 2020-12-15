using System;

using Minsk.CodeAnalysis.Diagnostic;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Binding
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