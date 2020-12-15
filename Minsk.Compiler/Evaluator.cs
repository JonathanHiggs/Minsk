using System;

using Minsk.Compiler.Binding;

namespace Minsk.Compiler
{
    internal sealed class Evaluator
    {
        private BoundExpression root;

        public Evaluator(BoundExpression root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public static bool Eval(BoundExpression root, out object value)
        {
            try
            {
                var evaluator = new Evaluator(root);
                value = evaluator.Evaluate();
                return true;
            }
            catch (Exception ex)
            {
                var foreground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;

                Console.WriteLine(ex.Message);

                Console.ForegroundColor = foreground;
            }

            value = 0;
            return false;
        }

        public object Evaluate()
        {
            return EvaluateExpression(root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch {
                BoundNodeKind.UnaryExpression       => EvaluateUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression      => EvaluateBinaryExpression(node as BoundBinaryExpression),
                BoundNodeKind.LiteralExpression     => (node as BoundLiteralExpression).Value,

                _ => throw new NotImplementedException(
                    $"{node.Kind} not implemented in EvaluateExpression")
            };
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression node)
        {
            var value = EvaluateExpression(node.Operand);
            var op = node.Op.Kind;

            return op switch {
                BoundUnaryOperatorKind.Identity => (int)value,
                BoundUnaryOperatorKind.Negation => -(int)value,

                BoundUnaryOperatorKind.LogicalNegation => !(bool)value,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateUnaryExpression")
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression node)
        {
            var left  = EvaluateExpression(node.Left);
            var right = EvaluateExpression(node.Right);

            var op = node.Op.Kind;

            return op switch {
                BoundBinaryOperatorKind.Addition        => (int)left + (int)right,
                BoundBinaryOperatorKind.Subtraction     => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication  => (int)left * (int)right,
                BoundBinaryOperatorKind.Division        => (int)left / (int)right,

                BoundBinaryOperatorKind.LogicalAnd      => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr       => (bool)left || (bool)right,

                BoundBinaryOperatorKind.Equals          => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals       => !Equals(left, right),

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateBinaryExpression")
            };
        }

    }
}