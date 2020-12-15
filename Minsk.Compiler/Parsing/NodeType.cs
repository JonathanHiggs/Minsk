namespace Minsk.Compiler.Parsing
{
    // ToDo: rename to NodeKind
    public enum NodeType
    {
        UnaryExpression,
        BinaryExpression,
        NumberLiteral,  // ToDo: rename LiteralExpression
        ParenthesesExpression,
        Operator,
    }
}