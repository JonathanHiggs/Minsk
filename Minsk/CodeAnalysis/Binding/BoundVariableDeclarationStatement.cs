using System.Collections.Generic;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Initializer = expression;

            Initializer.Parent = this;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                yield return Initializer;
            }
        }

        protected override string PrettyPrintText()
            => $"{Variable.Type.Name}:{Variable.Name}";
    }
}