using System;
using System.CodeDom.Compiler;
using System.IO;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;
using Minsk.IO;

namespace Minsk.CodeAnalysis.Binding
{
    internal static class BoundNodePrinter
    {
        public static void WriteTo(this BoundNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
                WriteTo(node, iw);
            else
                WriteTo(node, new IndentedTextWriter(writer));
        }

        public static void WriteTo(this BoundNode node, IndentedTextWriter writer)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression(node as BoundAssignmentExpression, writer);
                    break;

                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression(node as BoundBinaryExpression, writer);
                    break;

                case BoundNodeKind.BlockStatement:
                    WriteBlockStatement(node as BoundBlockStatement, writer);
                    break;

                case BoundNodeKind.CallExpression:
                    WriteCallExpression(node as BoundCallExpression, writer);
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement(node as BoundConditionalGotoStatement, writer);
                    break;

                case BoundNodeKind.ConditionalStatement:
                    WriteConditionalStatement(node as BoundConditionalStatement, writer);
                    break;

                case BoundNodeKind.ConversionExpression:
                    WriteCoversionExpression(node as BoundConversionExpression, writer);
                    break;

                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression(node as BoundErrorExpression, writer);
                    break;

                case BoundNodeKind.ErrorStatement:
                    WriteErrorStatement(node as BoundErrorStatement, writer);
                    break;

                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement(node as BoundExpressionStatement, writer);
                    break;

                case BoundNodeKind.ForToStatement:
                    WriteForToStatement(node as BoundForToStatement, writer);
                    break;

                case BoundNodeKind.GotoStatement:
                    WriteGotoStatement(node as BoundGotoStatement, writer);
                    break;

                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement(node as BoundLabelStatement, writer);
                    break;

                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression(node as BoundLiteralExpression, writer);
                    break;

                case BoundNodeKind.ReturnStatement:
                    WriteReturnStatement(node as BoundReturnStatement, writer);
                    break;

                case BoundNodeKind.Program:
                    WriteProgram(node as BoundProgram, writer);
                    break;

                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression(node as BoundUnaryExpression, writer);
                    break;

                case BoundNodeKind.VariableDeclarationStatement:
                    WriteVariableDeclarationStatement(node as BoundVariableDeclarationStatement, writer);
                    break;

                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression(node as BoundVariableExpression, writer);
                    break;

                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement(node as BoundWhileStatement, writer);
                    break;

                default:
                    throw new Exception($"Unexpected node kind '{node.Kind}'");
            }
        }

        private static void WriteNestedStatement(this IndentedTextWriter writer, BoundStatement node)
        {
            var needsIndentation = !(node is BoundBlockStatement);

            if (needsIndentation)
                writer.Indent++;

            node.WriteTo(writer);

            if (needsIndentation)
                writer.Indent--;
        }

        private static void WriteNestedExpression(
            this IndentedTextWriter writer,
            int parentPrecedence,
            BoundExpression expression)
        {
            if (expression is BoundUnaryExpression unary)
                writer.WriteNestedExpression(
                    parentPrecedence, SyntaxFacts.UnaryOperatorPrecedence(unary.Op.TokenKind), unary);
            else if (expression is BoundBinaryExpression binary)
                writer.WriteNestedExpression(
                    parentPrecedence, SyntaxFacts.BinaryOperatorPrecedence(binary.Op.TokenKind), binary);
            else
                expression.WriteTo(writer);
        }

        private static void WriteNestedExpression(
            this IndentedTextWriter writer,
            int parentPrecedence,
            int currentPrecedence,
            BoundExpression expression)
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;

            if (needsParenthesis)
                writer.WritePunctuation(TokenKind.OpenParenthesis);

            expression.WriteTo(writer);

            if (needsParenthesis)
                writer.WritePunctuation(TokenKind.CloseParenthesis);
        }

        private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.Equals);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
        {
            var precedence = SyntaxFacts.BinaryOperatorPrecedence(node.Op.TokenKind);

            writer.WriteNestedExpression(precedence, node.Left);
            writer.WriteSpace();
            writer.WritePunctuation(node.Op.TokenKind);
            writer.WriteSpace();
            writer.WriteNestedExpression(precedence, node.Right);
        }

        private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(TokenKind.OpenBrace);
            writer.WriteLine();
            writer.Indent++;

            foreach (var statement in node.Statements)
                statement.WriteTo(writer);

            writer.Indent--;
            writer.WritePunctuation(TokenKind.CloseBrace);
            writer.WriteLine();
        }

        private static void WriteCallExpression(BoundCallExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Function.Name);
            writer.WritePunctuation(TokenKind.OpenParenthesis);

            var isFirst = true;
            foreach (var argument in node.Arguments)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation(TokenKind.Comma);
                    writer.WriteSpace();
                }

                argument.WriteTo(writer);
            }

            writer.WritePunctuation(TokenKind.CloseParenthesis);
        }

        private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto"); // There is no SyntaxKind for goto
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteSpace();
            writer.WriteKeyword(node.JumpIfTrue ? "if" : "unless");
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteConditionalStatement(BoundConditionalStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.IfKeyword);
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.ThenStatement);

            if (node.ElseStatement != null)
            {
                writer.WriteKeyword(TokenKind.ElseKeyword);
                writer.WriteLine();
                writer.WriteNestedStatement(node.ElseStatement);
            }
        }

        private static void WriteCoversionExpression(BoundConversionExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Type.Name);
            writer.WritePunctuation(TokenKind.OpenParenthesis);
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(TokenKind.CloseParenthesis);
        }

        private static void WriteErrorExpression(BoundErrorExpression node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("?");
        }

        private static void WriteErrorStatement(BoundErrorStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("?");
        }

        private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteForToStatement(BoundForToStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.ForKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.Equals);
            writer.WriteSpace();
            node.LowerBound.WriteTo(writer);
            writer.WriteSpace();
            writer.WriteKeyword(TokenKind.ToKeyword);
            writer.WriteSpace();
            node.UpperBound.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
        }

        private static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto"); // There is no SyntaxKind for goto
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
        {
            var unindent = writer.Indent > 0;
            if (unindent)
                writer.Indent--;

            writer.WritePunctuation(node.Label.Name);
            writer.WritePunctuation(TokenKind.Colon);
            writer.WriteLine();

            if (unindent)
                writer.Indent++;
        }

        private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
        {
            var value = node.Value.ToString()!;

            if (node.Type == TypeSymbol.Bool)
            {
                writer.WriteKeyword((bool)node.Value ? TokenKind.TrueKeyword : TokenKind.FalseKeyword);
            }
            else if (node.Type == TypeSymbol.Int)
            {
                writer.WriteNumber(value);
            }
            else if (node.Type == TypeSymbol.String)
            {
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
                writer.WriteString(value);
            }
            else
            {
                throw new Exception($"Unexpected type {node.Type}");
            }
        }

        private static void WriteReturnStatement(BoundReturnStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.ReturnKeyword);
            if (node.Expression is not null)
            {
                writer.WriteSpace();
                node.Expression.WriteTo(writer);
            }
            writer.WriteLine();
        }

        private static void WriteProgram(BoundProgram node, IndentedTextWriter writer)
        {
            foreach (var function in node.Functions)
            {
                var unindent = writer.Indent > 0;
                if (unindent)
                    writer.Indent--;

                writer.WritePunctuation(function.Key.Name);
                writer.WritePunctuation(TokenKind.Colon);
                writer.WriteLine();

                WriteBlockStatement(function.Value, writer);
                writer.WriteLine();
            }
        }

        private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
        {
            var precedence = SyntaxFacts.UnaryOperatorPrecedence(node.Op.TokenKind);

            writer.WritePunctuation(node.Op.TokenKind);
            writer.WriteNestedExpression(precedence, node.Operand);
        }

        private static void WriteVariableDeclarationStatement(BoundVariableDeclarationStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.Variable.IsReadOnly ? TokenKind.LetKeyword : TokenKind.VarKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.Equals);
            writer.WriteSpace();
            node.Initializer.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
        }

        private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.WhileKeyword);
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
        }
    }
}
