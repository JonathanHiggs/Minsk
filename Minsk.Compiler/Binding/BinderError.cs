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
        
        // ToDo: Link to token
        public override string ToString()
            => $"BindingError  \"{Node.Text}\"  {Message}";
    }
}