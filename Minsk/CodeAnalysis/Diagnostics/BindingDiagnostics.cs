using System;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public sealed class BindingDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal BindingDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void UndefinedOperator(SyntaxNode node, string message)
            => bag.Report(new BindError(node, message));
    }
}