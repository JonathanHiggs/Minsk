namespace Minsk.CodeAnalysis.Parsing
{
    public enum SyntaxKind
    {
        // Expressions
        AssignmentExpression,
        BinaryExpression,
        CallExpression,
        LiteralExpression,
        NameExpression,
        ParenthesesExpression,
        UnaryExpression,

        // Nodes
        CompilationUnit,
        ElseNode,
        TypeClause,

        // Statements
        BlockStatement,
        ConditionalStatement,
        ExpressionStatement,
        ForToStatement,
        VariableDeclaration,
        WhileStatement,
    }
}