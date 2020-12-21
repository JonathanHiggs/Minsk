using System;
using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(
            BoundExpression left,
            BoundBinaryOperator op,
            BoundExpression right)
        {
            Left = left;
            Op = op;
            Right = right;

            Left.Parent = this;
            Right.Parent = this;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override Type Type => Op.Type;
        public BoundExpression Left { get; }
        public BoundBinaryOperator Op { get; }
        public BoundExpression Right { get; }

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        protected override string PrettyPrintText()
            => $"{Op.Kind}, {Type.Name}";
    }
}