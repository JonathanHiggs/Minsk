using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression root;
        private readonly Dictionary<VariableSymbol, object> variables;

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));

            this.variables = variables
                ?? throw new ArgumentNullException(nameof(variables));
        }

        public object Evaluate()
            => EvaluateExpression(root);

        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch {
                BoundNodeKind.AssignmentExpression
                    => EvaluateAssignmentExpression(node as BoundAssignmentExpression),

                BoundNodeKind.BinaryExpression
                    => EvaluateBinaryExpression(node as BoundBinaryExpression),

                BoundNodeKind.LiteralExpression
                    => EvaluateLiteralExpression(node as BoundLiteralExpression),

                BoundNodeKind.VariableExpression
                    => EvaluateNameExpression(node as BoundVariableExpression),

                BoundNodeKind.UnaryExpression
                    => EvaluateUnaryExpression(node as BoundUnaryExpression),

                _   => throw new NotImplementedException(
                        $"{node.Kind} not implemented in EvaluateExpression")
            };
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            variables[node.Variable] = value;
            return value;
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

        private object EvaluateLiteralExpression(BoundLiteralExpression node)
            => node.Value;

        private object EvaluateNameExpression(BoundVariableExpression node)
            => variables[node.Variable];

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

    }
}