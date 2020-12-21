using System;
using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
        {
            Op = op;
            Operand = operand;

            Operand.Parent = this;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type Type => Op.Type;
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Operand;
            }
        }

        protected override string PrettyPrintText()
            => $"{Op.Kind}, {Type.Name}";
    }
}