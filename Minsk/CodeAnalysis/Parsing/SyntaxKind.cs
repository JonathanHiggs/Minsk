namespace Minsk.CodeAnalysis.Parsing
{
    public enum SyntaxKind
    {
        // Expressions
        AssignmentExpression,
        BinaryExpression,
        LiteralExpression,
        NameExpression,
        ParenthesesExpression,
        UnaryExpression,

        // Nodes
        CompilationUnit,
        ElseNode,

        // Statements
        BlockStatement,
        ConditionalStatement,
        ExpressionStatement,
        VariableDeclaration,
    }
}