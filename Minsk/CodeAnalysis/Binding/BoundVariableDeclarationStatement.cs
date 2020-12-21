using System.Collections.Generic;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;

            Expression.Parent = this;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;

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