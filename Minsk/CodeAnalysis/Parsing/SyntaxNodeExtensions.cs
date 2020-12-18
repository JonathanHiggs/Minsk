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
                case SyntaxKind.AssignmentExpression:
                {
                    var assignment = node as AssignmentExpression;
                    return new BinaryVisualNode(
                        assignment.EqualsToken.Text,
                        assignment.Kind.ToString(),
                        new TerminalVisualNode(
                            assignment.IdentifierToken.Text.ToString(),
                            assignment.IdentifierToken.Kind.ToString(),
                            settings),
                        assignment.Expression.ToVisualTree(settings),
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

                case SyntaxKind.NameExpression:
                {
                    var name = node as NameExpression;
                    return new TerminalVisualNode(
                        name.IdentifierToken.Value?.ToString() ?? "<unknown>",
                        name.Kind.ToString(),
                        settings);
                }

                case SyntaxKind.ParenthesesExpression:
                {
                    var parens = node as ParenthesizedExpression;
                    return parens.Expression.ToVisualTree(settings);
                }

                case SyntaxKind.UnaryExpression:
                {
                    var unary = node as UnaryExpression;
                    return new UnaryVisualNode(
                        unary.Text,
                        unary.OperatorToken.Kind.ToString(),
                        unary.Operand.ToVisualTree(settings),
                        settings);
                }

                case SyntaxKind.CompilationUnit:
                {
                    var unit = node as CompilationUnit;
                    return new UnaryVisualNode(
                        string.Empty,
                        "CompilationUnit",
                        unit.Statement.ToVisualTree(settings),
                        settings);
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}