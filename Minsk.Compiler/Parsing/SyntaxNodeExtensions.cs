using System;

using Minsk.Compiler.Diagnostic;

namespace Minsk.Compiler.Parsing
{
    public static class SyntaxNodeExtensions
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
            // ToDo: switch on node.NodeType
            return node switch
            {
                UnaryExpression unary
                    => new UnaryVisualNode(
                        unary.Text,
                        unary.OperatorNode.Kind.ToString(),
                        unary.Operand.ToVisualTree(settings),
                        settings),

                BinaryExpression binary 
                    => new BinaryVisualNode(
                        binary.Text, 
                        binary.OperatorNode.Kind.ToString(), 
                        binary.Left.ToVisualTree(settings), 
                        binary.Right.ToVisualTree(settings), 
                        settings),

                LiteralExpression numberNode
                    => new TerminalVisualNode(
                        numberNode.Text ?? "<empty>", 
                        numberNode.Token.Kind.ToString(),
                        settings),

                ParenthesizedExpression parensExpression
                    => parensExpression.Expression.ToVisualTree(settings),

                OperatorNode operatorNode
                    => throw new InvalidOperationException(),

                _ => throw new InvalidOperationException()
            };
        }
    }
}