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

        public void CannotConvert(SyntaxNode node, TextSpan span, TypeSymbol expressionType, TypeSymbol targetType)
            => Error(
                node,
                span,
                $"Cannot convert from '{expressionType}' to '{targetType}'");

        public void CannotConvert(AssignmentExpression node, TypeSymbol expressionType, VariableSymbol variable)
            => Error(
                node,
                node.EqualsToken.Span,
                $"Cannot assign {expressionType} to {node.IdentifierToken.Text}:{variable.Type}");

        public void CannotImplicitlyConvert(SyntaxNode node, TextSpan span, TypeSymbol expressionType, TypeSymbol targetType)
            => Error(
                node,
                span,
                $"Cannot implicitly convert from '{expressionType}' to '{targetType}'. An explicit conversion exists (are you missing a cast?)");

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

        internal void UndefinedType(SyntaxNode node, LexToken identifier)
            => Error(node, identifier.Span, $"Type '{identifier.Text}' does not exist");

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

        public void ParameterAlreadyDeclared(ParameterSyntax node)
            => Error(node, node.Identifier.Span, $"A parameter with the name '{node.Identifier.Text}' was already declared");

        public void VoidExpression(Expression node)
            => Error(node, node.Span, "Expression must return a value");

        public void SymbolAlreadyDeclared(FunctionDeclaration node)
            => Error(node, node.Identifier.Span, $"A symbol with the name '{node.Identifier.Text}' was already declared");

        public void InvalidBreak(BreakStatement node)
            => Error(node, node.Span, $"Invalid break statement; not in a loop");

        public void InvalidContinue(ContinueStatement node)
            => Error(node, node.Span, $"Invalid continue statement; not in a loop");

        public void InvalidReturnType(ReturnStatement node, TypeSymbol expected, TypeSymbol actual)
            => Error(node, node.Expression.Span, $"Invalid return type; expected '{expected}' but was '{actual}'");

        public void UnexpectedReturnStatement(ReturnStatement node)
            => Error(node, node.ReturnKeyword.Span, $"Unexpected return statement");

        internal void MissingReturnExpression(ReturnStatement node)
            => Error(node, node.Span, $"Missing return expression");
    }
}