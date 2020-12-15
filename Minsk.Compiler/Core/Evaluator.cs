using System;

using Minsk.Compiler.Binding;
using Minsk.Compiler.Lexing;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Core
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
            // ToDo: switch on node.Kind to avoid multiple casts
            return node switch {
                BoundUnaryExpression unary           => EvaluateUnaryExpression(unary),
                BoundBinaryExpression binary         => EvaluateBinaryExpression(binary),
                BoundLiteralExpression literal       => literal.Value,

                _ => throw new NotImplementedException(
                    $"{node.Kind} not implemented in EvaluateExpression")
            };
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression node)
        {
            var value = (int)EvaluateExpression(node.Operand);
            var op = node.OperatorKind;

            return op switch {
                BoundUnaryOperatorKind.Identity => value,
                BoundUnaryOperatorKind.Negation => -value,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateUnaryExpression")
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression node)
        {
            var leftValue  = (int)EvaluateExpression(node.Left);
            var rightValue = (int)EvaluateExpression(node.Right);

            var op = node.OperatorKind;

            return op switch {
                BoundBinaryOperatorKind.Addition        => leftValue + rightValue,
                BoundBinaryOperatorKind.Subtraction     => leftValue - rightValue,
                BoundBinaryOperatorKind.Multiplication  => leftValue * rightValue,
                BoundBinaryOperatorKind.Division        => leftValue / rightValue,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateBinaryExpression")
            };
        }

    }
}