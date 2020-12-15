using System;
using System.Collections.Generic;

using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class Parser
    {
        private int position = 0;
        private List<LexToken> tokens;
        private List<CompilerError> errors = new List<CompilerError>();

        public Parser(string text)
        {
            tokens = new List<LexToken>(text.Length / 4);
            var lexer = new Lexer(text);

            while(lexer.HasNext)
            {
                var token = lexer.NextToken();

                if (token.Kind == TokenKind.Whitespace)
                    continue;

                tokens.Add(token);
            }

            errors.AddRange(lexer.Errors);
        }

        public bool HasErrors => errors.Count > 0;

        public IEnumerable<CompilerError> Errors => errors;

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokenKind.EoF);
            return new SyntaxTree(expression, eof, errors);
        }

        private Expression ParseExpression(int parentPrecedence = 0)
        {
            Expression left;
            var unaryPrecedence = Current.Kind.UnaryOperatorPrecedence();

            if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryPrecedence);
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
                var right = ParseExpression(binaryPrecendence);
                
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

            AddError(Current, $"Unexpected token. Expected '{tokenType}' but was '{Current.Kind}'");
            
            return new LexToken(tokenType, Current.Position, null, null);
        }

        private void AddError(LexToken token, string message)
        {
            errors.Add(new SyntaxError(token, message));
        }
    }
}