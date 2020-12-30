using System;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;
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

        public void ArgumentTypeMismatch(Expression node, TypeSymbol argumentType, ParameterSymbol parameter)
            => Error(
                node,
                node.Span,
                $"Argument type '{argumentType}' does not match the '{parameter.Name}' parameter type '{parameter.Type}'");

        public void CannotAssignToReadOnlyVariable(AssignmentExpression node)
            => Error(
                node,
                node.IdentifierToken.Span,
                $"Cannot assign to a read-only variable");

        public void CannotConvert(Expression node, TextSpan span, TypeSymbol expressionType, TypeSymbol targetType)
            => Error(
                node,
                span,
                $"{expressionType} to {targetType}");

        public void CannotConvert(AssignmentExpression node, TypeSymbol expressionType, VariableSymbol variable)
            => Error(
                node,
                node.EqualsToken.Span,
                $"Cannot assign {expressionType} to {node.IdentifierToken.Text}:{variable.Type}");

        public void MismatchingArgumentCount(CallExpression node, FunctionSymbol function)
            => Error(
                node,
                node.OpenParens.Span.To(node.CloseParens.Span),
                $"Function '{function.Name}' requires {function.Parameters.Length} parameters");

        internal void UndefinedFunction(CallExpression node)
            => Error(node, node.Identifier.Span, "UndefinedFunction");

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

        public void VariableRedeclaration(SyntaxNode node, LexToken identifierToken)
            => Error(
                node,
                identifierToken.Span,
                $"Variable {identifierToken.Text} already declared");

        public void VariableRedeclaration(ForToStatement node)
            => Error(
                node,
                node.Identifier.Span,
                $"Variable {node.Identifier.Text} already declared");

        public void VoidExpression(Expression node)
            => Error(node, node.Span, "Expression must return a value");
    }
}