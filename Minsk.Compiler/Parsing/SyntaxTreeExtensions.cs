using System;

using Minsk.Compiler.Diagnostic;

namespace Minsk.Compiler.Parsing
{
    public static class SyntaxTreeExtensions
    {
        public static VisualNode ToVisualTree(this SyntaxTree tree)
            => tree.Root.ToVisualTree();

        public static VisualNode ToVisualTree(this SyntaxTree tree, Action<VisualTreeSettings> config)
            => tree.Root.ToVisualTree(config);

        public static VisualNode ToVisualTree(this SyntaxTree tree, VisualTreeSettings settings)
            => tree.Root.ToVisualTree(settings);
    }
}