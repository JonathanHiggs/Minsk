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

        public void UndeclaredIdentifier(NameExpression node)
            => Error(
                node,
                node.IdentifierToken.Span,
                $"Undefined identifier {node.IdentifierToken.Text}");

        public void UndeclaredIdentifier(AssignmentExpression node)
            => Error(
                node,
                node.IdentifierToken.Span,
                $"Undeclared identifier {node.IdentifierToken.Text}");

        public void VariableRedeclaration(VariableDeclarationStatement node)
            => Error(
                node,
                node.IdentifierToken.Span,
                $"Variable {node.IdentifierToken.Text} already declared");

        public void CannotConvert(AssignmentExpression node, Type expressionType, VariableSymbol variable)
            => Error(
                node,
                node.EqualsToken.Span,
                $"Cannot assign {expressionType} to {node.IdentifierToken.Text}:{variable.Type}");

        public void CannotAssignToReadOnlyVariable(AssignmentExpression node)
            => Error(
                node,
                node.IdentifierToken.Span,
                $"Cannot assign to a read-only variable");
    }
}