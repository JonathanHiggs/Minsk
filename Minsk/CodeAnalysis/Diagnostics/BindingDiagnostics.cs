using System;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Diagnostics
{
    // ToDo: should this live in the Diagnostics or Binding namespace
    public sealed class BindingDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal BindingDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void UndefinedOperator(SyntaxNode node, TextSpan span, string message)
            => bag.Report(new BindError(node, span, message));

        public void UndefinedIdentifier(NameExpression nameExpression)
            => bag.Report(new BindError(
                nameExpression,
                nameExpression.IdentifierToken.Span,
                $"Undefined identifier {nameExpression.IdentifierToken.Text}"));
    }
}