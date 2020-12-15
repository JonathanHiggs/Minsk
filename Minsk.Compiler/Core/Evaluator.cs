using System;
using Minsk.Compiler.Lexing;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Core
{
    public sealed class Evaluator
    {
        private Expression root;

        public Evaluator(Expression root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public static bool Eval(Expression root, out int value)
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

        public int Evaluate()
        {
            return EvaluateExpression(root);
        }

        private int EvaluateExpression(Expression node)
        {
            // ToDo: switch on node.NodeType to avoid multiple casts
            return node switch {
                UnaryExpression unary           => EvaluateUnaryExpression(unary),
                BinaryExpression binary         => EvaluateBinaryExpression(binary),
                NumberLiteral number            => (int)number.NumberToken.Value,
                ParenthesizedExpression parens  => EvaluateExpression(parens.Expression),

                _ => throw new NotImplementedException(
                    $"{node.NodeType.ToString()} not implemented in EvaluateExpression")
            };
        }

        private int EvaluateUnaryExpression(UnaryExpression node)
        {
            var value = EvaluateExpression(node.Operand);
            var op = node.OperatorNode.Token.TokenType;

            return op switch {
                TokenType.Plus  => value,
                TokenType.Minus => -value,

                _ => throw new NotImplementedException(
                    $"{op} not implemented in EvaluateUnaryExpression")
            };
        }


        private int EvaluateBinaryExpression(BinaryExpression node)
        {
            var leftValue = EvaluateExpression(node.Left);
            var rightValue = EvaluateExpression(node.Right);

            var op = node.OperatorNode.Token.TokenType;

            return op switch {
                TokenType.Plus          => leftValue + rightValue,
                TokenType.Minus         => leftValue - rightValue,
                TokenType.Star          => leftValue * rightValue,
                TokenType.ForwardSlash  => leftValue / rightValue,

                _ => throw new NotImplementedException(
                    $"{op} not implemented in EvaluateBinaryExpression")
            };
        }

    }
}