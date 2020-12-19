using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundStatement root;
        private readonly Dictionary<VariableSymbol, object> variables;
        private object lastValue = null;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));

            this.variables = variables
                ?? throw new ArgumentNullException(nameof(variables));
        }

        public object Evaluate()
        {
            EvaluateStatement(root);
            return lastValue;
        }

        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement(node as BoundBlockStatement);
                    break;

                case BoundNodeKind.ConditionalStatement:
                    EvaluateConditionalStatement(node as BoundConditionalStatement);
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement(node as BoundExpressionStatement);
                    break;

                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclarationStatement(node as BoundVariableDeclarationStatement);
                    break;

                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement(node as BoundWhileStatement);
                    break;

                default:
                    throw new Exception();
            }
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.BoundStatements)
                EvaluateStatement(statement);
        }

        private void EvaluateConditionalStatement(BoundConditionalStatement node)
        {
            var condition = (bool)EvaluateExpression(node.Condition);
            if (condition)
                EvaluateStatement(node.ThenStatement);
            else if (node.ElseStatement is not null)
                EvaluateStatement(node.ElseStatement);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            lastValue = EvaluateExpression(node.Expression);
        }

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            var value = EvaluateExpression(node.Expression);
            variables[node.Variable] = value;
            lastValue = value;
        }

        private void EvaluateWhileStatement(BoundWhileStatement node)
        {
            while((bool)EvaluateExpression(node.Condition))
                EvaluateStatement(node.Body);
        }

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
                BoundBinaryOperatorKind.Addition        => (int)left  +  (int)right,
                BoundBinaryOperatorKind.Subtraction     => (int)left  -  (int)right,
                BoundBinaryOperatorKind.Multiplication  => (int)left  *  (int)right,
                BoundBinaryOperatorKind.Division        => (int)left  /  (int)right,

                BoundBinaryOperatorKind.LogicalAnd      => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr       => (bool)left || (bool)right,
                BoundBinaryOperatorKind.Less            => (int)left  <  (int)right,
                BoundBinaryOperatorKind.LessOrEquals    => (int)left  <= (int)right,
                BoundBinaryOperatorKind.Greater         => (int)left  >  (int)right,
                BoundBinaryOperatorKind.GreaterOrEquals => (int)left  >= (int)right,

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