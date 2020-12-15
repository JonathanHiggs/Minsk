using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Binding
{
    public sealed class BindError : Diagnostic
    {
        public BindError(SyntaxNode node, string message)
            : base(
                node.FirstToken.Position,
                node.LastToken.Position + (node.LastToken.Text?.Length ?? 0) - node.FirstToken.Position,
                message)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public SyntaxNode Node { get; }
        
        public override string ToString()
            => $"BindingError  "
             + $"{Source.Start}-{Source.Start + Source.Length}  "
             + $"\"{Node.LongText}\"  {Message}";
    }
}