using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;

            Expression.Parent = this;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public override Type Type => Expression.Type;
        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Expression;
            }
        }

        protected override string PrettyPrintText()
            => $"{Variable.Name}, {Variable.Type.Name}";
    }
}