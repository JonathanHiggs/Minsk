using System;
using Minsk.Compiler.Lexing;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Core
{
    public class Evaluator
    {
        private ExpressionSyntaxNode root;

        public Evaluator(ExpressionSyntaxNode root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public static bool Eval(ExpressionSyntaxNode root, out int value)
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

        private int EvaluateExpression(ExpressionSyntaxNode node)
        {
            return node switch {
                BinaryExpressionNode binary => EvaluateBinaryExpression(binary),
                NumberSyntaxNode number     => (int)number.NumberToken.Value,

                _ => throw new NotImplementedException(
                    $"{node.NodeType.ToString()} not implemented in EvaluateExpression")
            };
        }

        private int EvaluateBinaryExpression(BinaryExpressionNode node)
        {
            var leftValue = EvaluateExpression(node.Left);
            var rightValue = EvaluateExpression(node.Right);

            var op = node.OperatorNode.Token.TokenType;

            return op switch {
                TokenType.Plus  => leftValue + rightValue,
                TokenType.Minus => leftValue - rightValue,

                _ => throw new NotImplementedException(
                    $"{op} not implemented in EvaluateBinaryExpression")
            };
        }

    }
}