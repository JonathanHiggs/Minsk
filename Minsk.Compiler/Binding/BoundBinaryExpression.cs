using System;

namespace Minsk.Compiler.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(
            BoundExpression left, 
            BoundBinaryOperatorKind op, 
            BoundExpression right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override Type Type => Right.Type;
        public BoundExpression Left { get; }
        public BoundBinaryOperatorKind Op { get; }
        public BoundExpression Right { get; }
    }
}