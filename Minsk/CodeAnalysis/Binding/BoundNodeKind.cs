namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        AssignmentExpression,
        BinaryExpression,
        CallExpression,
        ConversionExpression,
        ErrorExpression,
        LiteralExpression,
        UnaryExpression,
        VariableExpression,

        // Statements
        BlockStatement,
        ConditionalStatement,
        ConditionalGotoStatement,
        ErrorStatement,
        ExpressionStatement,
        ForToStatement,
        GotoStatement,
        LabelStatement,
        VariableDeclarationStatement,
        WhileStatement,

        // High-level
        Program,
    }
}