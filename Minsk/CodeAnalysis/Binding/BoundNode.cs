using System;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal abstract class BoundNode : Node<BoundNode, BoundNodeKind>
    {
        protected override ConsoleColor PrettyPrintColorForKind(BoundNodeKind kind)
            => Console.ForegroundColor;
    }
}