using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundBlockStatement root;
        private readonly Dictionary<VariableSymbol, object> variables;
        private object lastValue = null;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));

            this.variables = variables
                ?? throw new ArgumentNullException(nameof(variables));
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (var i = 0; i < root.Statements.Length; i++)
            {
                if (root.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }

            var index = 0;

            while (index < root.Statements.Length)
            {
                var s = root.Statements[index];

                switch (s.Kind)
                {
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = s as BoundConditionalGotoStatement;
                        var condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;

                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement(s as BoundExpressionStatement);
                        index++;
                        break;

                    case BoundNodeKind.GotoStatement:
                        var gs = s as BoundGotoStatement;
                        index = labelToIndex[gs.Label];
                        break;

                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;

                    case BoundNodeKind.VariableDeclarationStatement:
                        EvaluateVariableDeclarationStatement(s as BoundVariableDeclarationStatement);
                        index++;
                        break;

                    default:
                        throw new NotSupportedException(s.Kind.ToString());
                }
            }

            //EvaluateStatement(root);
            return lastValue;
        }

        //private void EvaluateStatement(BoundStatement node)
        //{
        //    switch (node.Kind)
        //    {
        //        case BoundNodeKind.BlockStatement:
        //            EvaluateBlockStatement(node as BoundBlockStatement);
        //            break;
        //
        //        case BoundNodeKind.ConditionalStatement:
        //            throw new InvalidOperationException("If statements should have been lowered");
        //            //EvaluateConditionalStatement(node as BoundConditionalStatement);
        //            //break;
        //
        //        case BoundNodeKind.ExpressionStatement:
        //            EvaluateExpressionStatement(node as BoundExpressionStatement);
        //            break;
        //
        //        case BoundNodeKind.ForToStatement:
        //            throw new InvalidOperationException("ForTo statements should have been lowered");
        //            //EvaluateForToStatement(node as BoundForToStatement);
        //            //break;
        //
        //        case BoundNodeKind.VariableDeclarationStatement:
        //            EvaluateVariableDeclarationStatement(node as BoundVariableDeclarationStatement);
        //            break;
        //
        //        case BoundNodeKind.WhileStatement:
        //            throw new InvalidOperationException("While statements should have been lowered");
        //            //EvaluateWhileStatement(node as BoundWhileStatement);
        //            //break;
        //
        //        default:
        //            throw new Exception();
        //    }
        //}

        //private void EvaluateBlockStatement(BoundBlockStatement node)
        //{
        //    foreach (var statement in node.Statements)
        //        EvaluateStatement(statement);
        //}

        //private void EvaluateConditionalStatement(BoundConditionalStatement node)
        //{
        //    var condition = (bool)EvaluateExpression(node.Condition);
        //    if (condition)
        //        EvaluateStatement(node.ThenStatement);
        //    else if (node.ElseStatement is not null)
        //        EvaluateStatement(node.ElseStatement);
        //}

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            lastValue = EvaluateExpression(node.Expression);
        }

        //private void EvaluateForToStatement(BoundForToStatement node)
        //{
        //    var lowerBound = (int)EvaluateExpression(node.LowerBound);
        //    var upperBound = (int)EvaluateExpression(node.UpperBound);
        //
        //    for (var value = lowerBound; value <= upperBound; value++)
        //    {
        //        variables[node.Variable] = value;
        //        EvaluateStatement(node.Body);
        //    }
        //}

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            var value = EvaluateExpression(node.Initializer);
            variables[node.Variable] = value;
            lastValue = value;
        }

        //private void EvaluateWhileStatement(BoundWhileStatement node)
        //{
        //    while((bool)EvaluateExpression(node.Condition))
        //        EvaluateStatement(node.Body);
        //}

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

                BoundBinaryOperatorKind.BitwiseAnd when node.Left.Type == typeof(int)   => (int) left & (int) right,
                BoundBinaryOperatorKind.BitwiseOr  when node.Left.Type == typeof(int)   => (int) left | (int) right,
                BoundBinaryOperatorKind.BitwiseXor when node.Left.Type == typeof(int)   => (int) left ^ (int) right,

                BoundBinaryOperatorKind.BitwiseAnd when node.Left.Type == typeof(bool)  => (bool)left & (bool)right,
                BoundBinaryOperatorKind.BitwiseOr  when node.Left.Type == typeof(bool)  => (bool)left | (bool)right,
                BoundBinaryOperatorKind.BitwiseXor when node.Left.Type == typeof(bool)  => (bool)left ^ (bool)right,

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

                BoundUnaryOperatorKind.OnesCompliment => ~(int)value,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateUnaryExpression")
            };
        }

    }
}