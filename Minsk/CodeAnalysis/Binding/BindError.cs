using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Binding
{
    public sealed class BindError : Diagnostic
    {
        public BindError(SyntaxNode node, TextLocation location, string message)
            : base(location, message)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        // ToDo: Add BindErrorKind ErrorKind

        public override DiagnosticKind Kind => DiagnosticKind.BindError;

        public SyntaxNode Node { get; }

        public override string ToString() => $"BindingError  {Message}";
    }
}