using System;

using Minsk.CodeAnalysis.Diagnostics.Visualization;

namespace Minsk.CodeAnalysis.Parsing
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
            switch (node.Kind)
            {
                case SyntaxKind.UnaryExpression:
                {
                    var unary = node as UnaryExpression;
                    return new UnaryVisualNode(
                        unary.Text,
                        unary.OperatorToken.Kind.ToString(),
                        unary.Operand.ToVisualTree(settings),
                        settings);
                }

                case SyntaxKind.BinaryExpression:
                {
                    var binary = node as BinaryExpression;
                    return new BinaryVisualNode(
                        binary.Text, 
                        binary.OperatorToken.Kind.ToString(), 
                        binary.Left.ToVisualTree(settings), 
                        binary.Right.ToVisualTree(settings), 
                        settings);
                }

                case SyntaxKind.LiteralExpression:
                {
                    var literal = node as LiteralExpression;
                    return new TerminalVisualNode(
                        literal.Text ?? "<empty>", 
                        literal.Token.Kind.ToString(),
                        settings);
                }

                case SyntaxKind.ParenthesesExpression:
                {
                    var parens = node as ParenthesizedExpression;
                    return parens.Expression.ToVisualTree(settings);
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}