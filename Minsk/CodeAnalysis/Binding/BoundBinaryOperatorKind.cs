namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        // Numerical
        Addition,
        Subtraction,
        Multiplication,
        Division,

        // Logical
        LogicalAnd,
        LogicalOr,

        Equals,
        NotEquals
    }
}