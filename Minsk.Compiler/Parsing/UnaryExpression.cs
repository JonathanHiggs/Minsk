using System;
using System.Collections.Generic;

namespace Minsk.Compiler.Parsing
{
    public sealed class UnaryExpression : Expression
    {
        public UnaryExpression(OperatorNode operatorNode, Expression operand)
        {
            OperatorNode = operatorNode
                ?? throw new ArgumentNullException(nameof(operatorNode));

            Operand = operand
                ?? throw new ArgumentNullException(nameof(operand));

            OperatorNode.Parent = this;
            Operand.Parent = this;
        }


        public OperatorNode OperatorNode { get; }
        public Expression Operand { get; }

        public override NodeType NodeType => NodeType.UnaryExpression;

        public override string Text => OperatorNode.Text;

        public override IEnumerable<SyntaxNode> Children 
        {
            get
            {
                yield return OperatorNode;
                yield return Operand;
            }
        }
    }
}