using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int labelCount = 1;

        private Lowerer()
        { }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);
            return lowerer.Flattern(result);
        }

        private BoundBlockStatement Flattern(BoundStatement node)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteConditionalStatement(BoundConditionalStatement node)
        {
            if (node.ElseStatement is null)
            {
                // if <condition> <then>
                // --- to --->
                // {
                // gotoFalse <condition> end
                // <then>
                // end:
                // }

                var endLabel = GenerateLabel("end");
                var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
                var endLabelStatement = new BoundLabelStatement(endLabel);

                var result = new BoundBlockStatement(
                    gotoFalse,
                    node.ThenStatement,
                    endLabelStatement);

                return RewriteStatement(result);
            }
            else
            {
                // if <condition> <then> else <else>
                // --- to --->
                // {
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:
                // }

                var endLabel = GenerateLabel("end");
                var elseLabel = GenerateLabel("else");

                var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
                var gotoEnd = new BoundGotoStatement(endLabel);
                var elseLabelStatement = new BoundLabelStatement(elseLabel);
                var endLabelStatement = new BoundLabelStatement(endLabel);

                var result = new BoundBlockStatement(
                    gotoFalse,
                    node.ThenStatement,
                    gotoEnd,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement);

                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteForToStatement(BoundForToStatement node)
        {
            // for <var> = <lower> to <upper> <body>
            // --- to --->
            // {
            // var <var> = <lower>
            // let upperBound = <upper>
            // while <var> <= upperBound
            //     <body>
            //     <var> = <var> + 1
            // }

            var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, node.LowerBound);
            var upperBoundSymbol = new VariableSymbol("upperBound", true, TypeSymbol.Int);
            var upperBoundDeclaration = new BoundVariableDeclarationStatement(upperBoundSymbol, node.UpperBound);
            var upperBoundExpression = new BoundVariableExpression(upperBoundSymbol);

            var condition = new BoundBinaryExpression(
                new BoundVariableExpression(node.Variable),
                BoundBinaryOperator.Bind(Lexing.TokenKind.LessOrEquals, TypeSymbol.Int, TypeSymbol.Int),
                upperBoundExpression);

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        new BoundVariableExpression(node.Variable),
                        BoundBinaryOperator.Bind(Lexing.TokenKind.Plus, TypeSymbol.Int, TypeSymbol.Int),
                        new BoundLiteralExpression(1))));

            var whileBlock =
                new BoundBlockStatement(
                    node.Body,
                    increment);

            var whileStatement = new BoundWhileStatement(condition, whileBlock);

            var result = new BoundBlockStatement(
                variableDeclaration,
                upperBoundDeclaration,
                whileStatement);

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // while <condition> <body>
            // --- to --->
            // {
            // goto check
            // continue:
            // <body>
            // check:
            // gotoTrue <condition> continue
            // end:
            // }

            var continueLabel = GenerateLabel("continue");
            var checkLabel = GenerateLabel("check");
            var endLabel = GenerateLabel("end");

            var gotoCheck = new BoundGotoStatement(checkLabel);
            var continueStatement = new BoundLabelStatement(continueLabel);
            var checkStatement = new BoundLabelStatement(checkLabel);
            var gotoTrueStatement = new BoundConditionalGotoStatement(continueLabel, node.Condition, true);
            var endStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(
                gotoCheck,
                continueStatement,
                node.Body,
                checkStatement,
                gotoTrueStatement,
                endStatement);

            return RewriteStatement(result);
        }

        private BoundLabel GenerateLabel(string name)
        {
            var id = $"Label-{name}-{labelCount++}";
            return new BoundLabel(id);
        }
    }
}