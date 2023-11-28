using System.Collections.Generic;

using Minsk.CodeAnalysis.Symbols;

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
        public override TypeSymbol Type => Op.Type;
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
            => $"{Type.Name}.{Op.Kind}";

        public bool OperandTypesAre(TypeSymbol type)
            => Left.Type == type && Right.Type == type;

        public bool OperandTypesAre(TypeSymbol left, TypeSymbol right)
            => Left.Type == left && Right.Type == right;
    }
}