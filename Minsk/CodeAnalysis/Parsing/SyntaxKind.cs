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
        BreakStatement,
        ConditionalStatement,
        ContinueStatement,
        ExpressionStatement,
        ForToStatement,
        ReturnStatement,
        VariableDeclaration,
        WhileStatement,

        // Top-level statements
        GlobalStatement,
        FunctionDeclaration,

        // Definitions
        Parameter,
    }
}