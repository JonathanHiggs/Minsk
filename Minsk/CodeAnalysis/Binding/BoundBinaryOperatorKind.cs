namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        // Numerical
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Less,
        LessOrEquals,
        Greater,
        GreaterOrEquals,

        // Logical
        LogicalAnd,
        LogicalOr,

        Equals,
        NotEquals,

        // Bitwise
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
    }
}