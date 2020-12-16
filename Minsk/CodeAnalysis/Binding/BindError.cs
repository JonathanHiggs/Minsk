using System;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Binding
{
    public sealed class BindError : Diagnostic
    {
        public BindError(SyntaxNode node, TextSpan span, string message)
            : base(span, message)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        // ToDo: Add BindErrorKind ErrorKind

        public override DiagnosticKind Kind => DiagnosticKind.BindError;

        public SyntaxNode Node { get; }

        public override string ToString()
            => $"BindingError  "
             + $"{Source.Start}-{Source.Start + Source.Length}  "
             + $"\"{Node.LongText}\"  {Message}";
    }
}