using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundCompilationUnit : BoundNode
    {
        public override IEnumerable<BoundNode> Children => throw new System.NotImplementedException();

        public override BoundNodeKind Kind => throw new System.NotImplementedException();

        protected override string PrettyPrintText()
        {
            throw new System.NotImplementedException();
        }
    }
}