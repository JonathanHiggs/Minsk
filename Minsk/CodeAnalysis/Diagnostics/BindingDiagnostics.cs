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


        private void Error(SyntaxNode node, string message)
            => Error(node, node.Location, message);

        private void Error(SyntaxNode node, TextLocation location, string message)
            => bag.Report(new BindError(node, location, message));


        public void ArgumentTypeMismatch(Expression node, TypeSymbol argumentType, ParameterSymbol parameter)
            => Error(node, $"Argument type '{argumentType}' does not match the '{parameter.Name}' parameter type '{parameter.Type}'");

        public void CannotAssignToReadOnlyVariable(AssignmentExpression node)
            => Error(node, $"Cannot assign to a read-only variable");

        public void CannotConvert(SyntaxNode node, TextLocation location, TypeSymbol expressionType, TypeSymbol targetType)
            => Error(node, location, $"Cannot convert from '{expressionType}' to '{targetType}'");

        public void CannotConvert(AssignmentExpression node, TypeSymbol expressionType, VariableSymbol variable)
            => Error(node, $"Cannot assign {expressionType} to {node.Identifier.Text}:{variable.Type}");

        public void CannotImplicitlyConvert(SyntaxNode node, TextLocation location, TypeSymbol expressionType, TypeSymbol targetType)
            => Error(node, location, $"Cannot implicitly convert from '{expressionType}' to '{targetType}'. An explicit conversion exists (are you missing a cast?)");

        public void MismatchingArgumentCount(CallExpression node, FunctionSymbol function)
            => Error(node, $"Function '{function.Name}' requires {function.Parameters.Length} parameters");

        internal void UndefinedFunction(CallExpression node)
            => Error(node, $"UndefinedFunction '{node.Identifier.Text}'");

        public void UndefinedOperator(SyntaxNode node, LexToken opToken, TypeSymbol type)
            => Error(node, $"No operator '{opToken.Text}' is defined that take operands of type '{type.Name}'");

        public void UndefinedOperator(SyntaxNode node, LexToken opToken, TypeSymbol leftType, TypeSymbol rightType)
            => Error(node, $"No operator '{opToken.Text}' is defined that take operands of type '{leftType.Name}' and '{rightType.Name}'");

        public void UndeclaredIdentifier(NameExpression node)
            => Error(node, $"Undefined identifier {node.IdentifierToken.Text}");

        internal void UndefinedType(SyntaxNode node, LexToken identifier)
            => Error(node, $"Type '{identifier.Text}' does not exist");

        public void UndeclaredIdentifier(AssignmentExpression node)
            => Error(node, $"Undeclared identifier {node.Identifier.Text}");

        public void AllPathsMustReturn(FunctionSymbol node)
            => Error(node.Declaration, $"All code paths must return a value");

        public void MultipleMainFunctions(FunctionDeclaration node)
            => Error(node, "Multiple 'Main' functions declared");

        public void VariableRedeclaration(SyntaxNode node, LexToken identifierToken)
            => Error(node, $"Variable {identifierToken.Text} already declared");

        public void VariableRedeclaration(ForToStatement node)
            => Error(node, $"Variable {node.Identifier.Text} already declared");

        internal void CannotMixMainAndGlobalStatements(FunctionDeclaration node)
            => Error(node, node.Identifier.Location, $"Cannot mix a main function and global statements");

        public void CannotMixMainAndGlobalStatements(GlobalStatementSyntax node)
            => Error(node, $"Cannot mix a main function and global statements");

        public void ParameterAlreadyDeclared(ParameterSyntax node)
            => Error(node, $"A parameter with the name '{node.Identifier.Text}' was already declared");

        public void VoidExpression(Expression node)
            => Error(node, "Expression must return a value");

        public void SymbolAlreadyDeclared(FunctionDeclaration node)
            => Error(node, node.Identifier.Location, $"A symbol with the name '{node.Identifier.Text}' was already declared");

        public void MultipleGlobalStatementFiles(GlobalStatementSyntax node)
            => Error(node, $"Only a single file may have global statements");

        public void InvalidBreak(BreakStatement node)
            => Error(node, $"Invalid break statement; not in a loop");

        public void MainCannotHaveParameters(FunctionDeclaration node)
            => Error(node, new TextLocation(node.SyntaxTree.Source, node.OpenParentheses.Location.Span.To(node.CloseParentheses.Location.Span)), $"Main method cannot have parameters");

        public void MainMustBeVoid(FunctionDeclaration node)
            => Error(node, node.OptionalTypeClause.Location, $"Main method must be void");

        public void InvalidContinue(ContinueStatement node)
            => Error(node, $"Invalid continue statement; not in a loop");

        public void InvalidReturnType(ReturnStatement node, TypeSymbol expected, TypeSymbol actual)
            => Error(node, $"Invalid return type; expected '{expected}' but was '{actual}'");

        public void UnexpectedReturnStatement(ReturnStatement node)
            => Error(node, $"Unexpected return statement");

        public void MissingReturnExpression(ReturnStatement node)
            => Error(node, $"Missing return expression");

        public void InvalidExpressionStatement(Statement node)
            => Error(node, $"Expression is invalid; only assignment and call expressions can be used as a statement");

        public void NotAFunction(CallExpression node, Symbol symbol)
            => Error(node, $"The identifier '{symbol.Name}' is not a function; previously defined as a {symbol.Kind}");

        public void NotAVariable(Expression node, TextLocation location, Symbol symbol)
            => Error(node, location, $"The identifier '{symbol.Name}' is not a variable; previously defined as a {symbol.Kind}");
    }
}