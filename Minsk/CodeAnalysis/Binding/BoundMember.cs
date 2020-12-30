using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Binding
{
    internal abstract class BoundMember : BoundNode
    {
    }

    internal sealed class BoundFunction : BoundMember
    {
        public override IEnumerable<BoundNode> Children => throw new System.NotImplementedException();

        public override BoundNodeKind Kind => throw new System.NotImplementedException();

        protected override string PrettyPrintText()
        {
            throw new System.NotImplementedException();
        }
    }

    internal sealed class BoundGlobalStatement : BoundMember
    {
        public override IEnumerable<BoundNode> Children => throw new System.NotImplementedException();

        public override BoundNodeKind Kind => throw new System.NotImplementedException();

        protected override string PrettyPrintText()
        {
            throw new System.NotImplementedException();
        }
    }
}