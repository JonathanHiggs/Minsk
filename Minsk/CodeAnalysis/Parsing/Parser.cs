using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class Parser
    {
        public readonly DiagnosticBag diagnostics;
        private List<LexToken> tokens;
        private int position = 0;

        public Parser(DiagnosticBag diagnostics, string text)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            tokens = new List<LexToken>(text.Length / 4);
            var lexer = new Lexer(diagnostics, text);

            while(lexer.HasNext)
            {
                var token = lexer.NextToken();

                if (token.Kind == TokenKind.Whitespace)
                    continue;

                tokens.Add(token);
            }
        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokenKind.EoF);
            return new SyntaxTree(expression, eof, diagnostics);
        }

        private Expression ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private Expression ParseAssignmentExpression()
        {
            if (PeekToken(0).Kind == TokenKind.Identifier
                && PeekToken(1).Kind == TokenKind.Equals)
            {
                var identifierToken = NextToken();
                var equalsToken = NextToken();
                var expression = ParseExpression();

                return new AssignmentExpression(identifierToken, equalsToken, expression);
            }

            return ParseBinaryExpression();
        }

        private Expression ParseBinaryExpression(int parentPrecedence = 0)
        {
            Expression left;
            var unaryPrecedence = Current.Kind.UnaryOperatorPrecedence();

            if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryPrecedence);
                left = new UnaryExpression(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var binaryPrecendence = Current.Kind.BinaryOperatorPrecedence();
                if (binaryPrecendence == 0 || binaryPrecendence <= parentPrecedence)
                    break;

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(binaryPrecendence);
                
                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case TokenKind.OpenParenthesis:
                {
                    var left = NextToken();
                    var expression = ParseExpression();
                    var right = MatchToken(TokenKind.CloseParenthesis);

                    return new ParenthesizedExpression(left, expression, right);
                }

                case TokenKind.FalseKeyword:
                case TokenKind.TrueKeyword:
                {
                    var value = Current.Kind == TokenKind.TrueKeyword;
                    return new LiteralExpression(NextToken(), value);
                }

                case TokenKind.Identifier:
                {
                    var identifierToke = NextToken();
                    return new NameExpression(identifierToke);
                }

                default:
                {
                    var numberToken = MatchToken(TokenKind.Number);
                    return new LiteralExpression(numberToken);
                }
            }
        }

        private LexToken PeekToken(int offset)
        {
            var index = position + offset;   

            if (index < 0)
                return tokens[0];

            if (index >= tokens.Count)
                return tokens[tokens.Count - 1];

            return tokens[index];
        }

        private LexToken Current => PeekToken(0);

        private LexToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private LexToken MatchToken(TokenKind tokenType)
        {
            if (Current.Kind == tokenType)
                return NextToken();

            diagnostics.Syntax.UnexpectedToken(Current, $"Expected '{tokenType}' but was '{Current.Kind}'");
            
            return new LexToken(tokenType, Current.Span, null, null);
        }
    }
}