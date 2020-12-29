using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;

using BinOp = Minsk.CodeAnalysis.Binding.BoundBinaryOperatorKind;
using UnaOp = Minsk.CodeAnalysis.Binding.BoundUnaryOperatorKind;

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
                BinOp.Addition        => (int)left  +  (int)right,
                BinOp.Subtraction     => (int)left  -  (int)right,
                BinOp.Multiplication  => (int)left  *  (int)right,
                BinOp.Division        => (int)left  /  (int)right,

                BinOp.LogicalAnd      => (bool)left && (bool)right,
                BinOp.LogicalOr       => (bool)left || (bool)right,
                BinOp.Less            => (int)left  <  (int)right,
                BinOp.LessOrEquals    => (int)left  <= (int)right,
                BinOp.Greater         => (int)left  >  (int)right,
                BinOp.GreaterOrEquals => (int)left  >= (int)right,

                BinOp.Equals          => Equals(left, right),
                BinOp.NotEquals       => !Equals(left, right),

                BinOp.BitwiseAnd when node.Left.Type == TypeSymbol.Int   => (int) left & (int) right,
                BinOp.BitwiseOr  when node.Left.Type == TypeSymbol.Int   => (int) left | (int) right,
                BinOp.BitwiseXor when node.Left.Type == TypeSymbol.Int   => (int) left ^ (int) right,
                BinOp.BitwiseAnd when node.Left.Type == TypeSymbol.Bool  => (bool)left & (bool)right,
                BinOp.BitwiseOr  when node.Left.Type == TypeSymbol.Bool  => (bool)left | (bool)right,
                BinOp.BitwiseXor when node.Left.Type == TypeSymbol.Bool  => (bool)left ^ (bool)right,

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
                UnaOp.Identity           => (int)value,
                UnaOp.Negation           => -(int)value,

                UnaOp.LogicalNegation    => !(bool)value,

                UnaOp.OnesCompliment     => ~(int)value,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateUnaryExpression")
            };
        }

    }
}