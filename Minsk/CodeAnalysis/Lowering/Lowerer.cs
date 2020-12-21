using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        { }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }

        protected override BoundStatement RewriteForToStatement(BoundForToStatement node)
        {
            // for <var> = <lower> to <upper> <body>
            // to
            // {
            // var <var> = <lower>
            // while <var> <= <upper>
            //     <body>
            //     <var> = <var> + 1
            // }

            var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, node.LowerBound);
            var condition = new BoundBinaryExpression(
                new BoundVariableExpression(node.Variable),
                BoundBinaryOperator.Bind(Lexing.TokenKind.LessOrEquals, typeof(int), typeof(int)),
                node.UpperBound);

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        new BoundVariableExpression(node.Variable),
                        BoundBinaryOperator.Bind(Lexing.TokenKind.Plus, typeof(int), typeof(int)),
                        new BoundLiteralExpression(1))));

            var whileBlock =
                new BoundBlockStatement(
                    node.Body,
                    increment);

            var whileStatement = new BoundWhileStatement(condition, whileBlock);

            return new BoundBlockStatement(
                variableDeclaration,
                whileStatement);
        }
    }
}