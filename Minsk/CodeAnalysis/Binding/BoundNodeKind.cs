namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        AssignmentExpression,
        BinaryExpression,
        LiteralExpression,
        UnaryExpression,
        VariableExpression,

        // Statements
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration
    }
}