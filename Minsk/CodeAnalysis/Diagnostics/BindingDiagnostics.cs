using System;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Diagnostics
{
    // ToDo: should this live in the Diagnostics or Binding namespace
    public sealed class BindingDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal BindingDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        private void Error(SyntaxNode node, TextSpan span, string message)
            => bag.Report(new BindError(node, span, message));

        public void UndefinedOperator(SyntaxNode node, TextSpan span, string message)
            => Error(node, span, message);

        public void UndefinedIdentifier(NameExpression nameExpression)
            => Error(
                nameExpression,
                nameExpression.IdentifierToken.Span,
                $"Undefined identifier {nameExpression.IdentifierToken.Text}");

        public void VariableRedeclaration(AssignmentExpression assignment)
            => Error(
                assignment,
                assignment.IdentifierToken.Span,
                $"Variable {assignment.IdentifierToken.Text} already declared");

        public void CannotConvert(AssignmentExpression assignment, Type expressionType, VariableSymbol variable)
            => Error(
                assignment,
                assignment.EqualsToken.Span,
                $"Can't assign {expressionType} to {variable.Type}");
    }
}