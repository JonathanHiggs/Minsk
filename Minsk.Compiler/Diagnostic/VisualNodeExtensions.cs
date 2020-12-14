using System;

namespace Minsk.Compiler.Diagnostic
{
    public static class VisualNodeExtensions
    {
        public static VisualNode ToVisualTree(this SyntaxNode node)
        {
            var settings = new VisualTreeSettings();
            return node.ToVisualTree(settings);
        }

        public static VisualNode ToVisualTree(this SyntaxNode node, Action<VisualTreeSettings> config)
        {
            var settings = new VisualTreeSettings();
            config(settings);
            return node.ToVisualTree(settings);
        }

        public static VisualNode ToVisualTree(this SyntaxNode node, VisualTreeSettings settings)
        {
            return node switch
            {
                BinaryExpressionNode binary 
                    => new BinaryVisualNode(binary.Text, binary.Left.ToVisualTree(settings), binary.Right.ToVisualTree(settings), settings),

                NumberSyntaxNode numberNode
                    => new TerminalVisualNode(numberNode.Text, settings),

                OperatorSyntaxNode operatorNode
                    => throw new InvalidOperationException(),

                _ => throw new InvalidOperationException()
            };
        }
    }
}