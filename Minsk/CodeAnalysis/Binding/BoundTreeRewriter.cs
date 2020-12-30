using System;
using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Binding
{
    internal class BoundTreeRewriter
    {
        public virtual BoundNode RewriteNode(BoundNode node)
        {
            //return node.Kind switch
            //{
            //    BoundNodeKind.AssignmentExpression
            //        => RewriteExpression
            //    BoundNodeKind.BinaryExpression
            //    BoundNodeKind.LiteralExpression
            //    BoundNodeKind.UnaryExpression
            //    BoundNodeKind.VariableExpression
            //
            //    BoundNodeKind.BlockStatement
            //    BoundNodeKind.ConditionalStatement
            //    BoundNodeKind.ExpressionStatement
            //    BoundNodeKind.ForToStatement
            //    BoundNodeKind.VariableDeclarationStatement
            //    BoundNodeKind.WhileStatement
            //}

            switch (node.Kind)
            {
                case BoundNodeKind.AssignmentExpression:
                case BoundNodeKind.BinaryExpression:
                case BoundNodeKind.CallExpression:
                case BoundNodeKind.ConversionExpression:
                case BoundNodeKind.LiteralExpression:
                case BoundNodeKind.UnaryExpression:
                case BoundNodeKind.VariableExpression:
                    return RewriteExpression(node as BoundExpression);

                case BoundNodeKind.BlockStatement:
                case BoundNodeKind.ConditionalStatement:
                case BoundNodeKind.ConditionalGotoStatement:
                case BoundNodeKind.ExpressionStatement:
                case BoundNodeKind.ForToStatement:
                case BoundNodeKind.GotoStatement:
                case BoundNodeKind.LabelStatement:
                case BoundNodeKind.VariableDeclarationStatement:
                case BoundNodeKind.WhileStatement:
                    return RewriteStatement(node as BoundStatement);

                default:
                    throw new NotImplementedException(node.Kind.ToString());
            }
        }

        protected virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            return node.Kind switch {
                BoundNodeKind.AssignmentExpression
                    => RewriteAssignmentExpression(node as BoundAssignmentExpression),

                BoundNodeKind.BinaryExpression
                    => RewriteBinaryExpression(node as BoundBinaryExpression),

                BoundNodeKind.CallExpression
                    => RewriteCallExpression(node as BoundCallExpression),

                BoundNodeKind.ConversionExpression
                    => RewriteConversionExpression(node as BoundConversionExpression),

                BoundNodeKind.ErrorExpression
                    => RewriteErrorExpression(node as BoundErrorExpression),

                BoundNodeKind.LiteralExpression
                    => RewriteLiteralExpression(node as BoundLiteralExpression),

                BoundNodeKind.UnaryExpression
                    => RewriteUnaryExpression(node as BoundUnaryExpression),

                BoundNodeKind.VariableExpression
                    => RewriteVariableExpression(node as BoundVariableExpression),

                _   => throw new NotImplementedException(node.Kind.ToString())
            };
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundAssignmentExpression(node.Variable, expression);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
                return node;

            return new BoundBinaryExpression(left, node.Op, right);
        }

        protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                var oldArgument = node.Arguments[i];
                var newArgument = RewriteExpression(oldArgument);

                if (newArgument != oldArgument)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);
                        for (var j = 0; j < i; j++)
                            builder.Add(node.Arguments[i]);
                    }
                }

                if (builder is not null)
                    builder.Add(newArgument);
            }

            if (builder is null)
                return node;

            return new BoundCallExpression(node.Function, builder.ToImmutable());
        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundConversionExpression(expression, node.Type);
        }

        protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
            => node;

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
            => node;

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var operand = RewriteExpression(node.Operand);
            if (operand == node.Operand)
                return node;

            return new BoundUnaryExpression(node.Op, operand);
        }

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
            => node;

        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            return node.Kind switch {
                BoundNodeKind.BlockStatement
                    => RewriteBlockStatement(node as BoundBlockStatement),

                BoundNodeKind.ConditionalStatement
                    => RewriteConditionalStatement(node as BoundConditionalStatement),

                BoundNodeKind.ConditionalGotoStatement
                    => RewriteConditionalGotoStatement(node as BoundConditionalGotoStatement),

                BoundNodeKind.ExpressionStatement
                    => RewriteExpressionStatement(node as BoundExpressionStatement),

                BoundNodeKind.ForToStatement
                    => RewriteForToStatement(node as BoundForToStatement),

                BoundNodeKind.GotoStatement
                    => RewriteGotoStatement(node as BoundGotoStatement),

                BoundNodeKind.LabelStatement
                    => RewriteLabelStatement(node as BoundLabelStatement),

                BoundNodeKind.VariableDeclarationStatement
                    => RewriteVariableDeclarationStatement(node as BoundVariableDeclarationStatement),

                BoundNodeKind.WhileStatement
                    => RewriteWhileStatement(node as BoundWhileStatement),

                _ => throw new NotImplementedException(node.Kind.ToString())
            };
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;
            var diff = false;

            for (var i = 0; i < node.Statements.Length; i++)
            {
                var original = node.Statements[i];
                var rewritten = RewriteStatement(original);

                if (rewritten != original && !diff)
                {
                    diff = true;
                    builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                    for (var j = 0; j < i; j++)
                        builder.Add(node.Statements[j]);
                }

                if (diff)
                    builder.Add(rewritten);
            }

            if (!diff)
                return node;

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected virtual BoundStatement RewriteConditionalStatement(BoundConditionalStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatement);
            var elseStatement =
                node.ElseStatement is not null ? RewriteStatement(node.ElseStatement) : null;

            if (condition == node.Condition
                && thenStatement == node.ThenStatement
                && (node.ElseStatement is null || elseStatement == node.ElseStatement))
                return node;

            return new BoundConditionalStatement(condition, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(
            BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
                return node;

            return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundExpressionStatement(expression);
        }

        protected virtual BoundStatement RewriteForToStatement(BoundForToStatement node)
        {
            var lowerBound = RewriteExpression(node.LowerBound);
            var upperBound = RewriteExpression(node.UpperBound);
            var body = RewriteStatement(node.Body);

            if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
                return node;

            return new BoundForToStatement(node.Variable, lowerBound, upperBound, body);
        }

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node)
            => node;

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node)
            => node;

        protected virtual BoundStatement RewriteVariableDeclarationStatement(
            BoundVariableDeclarationStatement node)
        {
            var initializer = RewriteExpression(node.Initializer);

            if (initializer == node.Initializer)
                return node;

            return new BoundVariableDeclarationStatement(node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);

            if (condition == node.Condition && body == node.Body)
                return node;

            return new BoundWhileStatement(condition, body);
        }
    }
}