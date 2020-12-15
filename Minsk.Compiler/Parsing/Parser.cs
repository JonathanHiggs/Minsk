using System;
using System.Collections.Generic;

using Minsk.Compiler.Core;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class Parser
    {
        private int position = 0;
        private List<SyntaxToken> tokens;
        private List<CompilerError> errors = new List<CompilerError>();

        public Parser(string text)
        {
            tokens = new List<SyntaxToken>(text.Length / 4);
            var lexer = new Lexer(text);

            while(lexer.HasNext)
            {
                var token = lexer.NextToken();

                if (token.TokenType == TokenType.Whitespace)
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
            var eof = MatchToken(TokenType.EoF);
            return new SyntaxTree(expression, eof, errors);
        }

        private Expression ParseExpression(int parentPrecedence = 0)
        {
            var left = ParsePrimaryExpression();

            while (true)
            {
                var precedence = Current.TokenType.BinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var operatorNode = new OperatorNode(NextToken());
                var right = ParseExpression(precedence);
                
                left = new BinaryExpression(left, operatorNode, right);
            }

            return left;
        }

        private Expression ParsePrimaryExpression()
        {
            if (Current.TokenType == TokenType.OpenParenthesis)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = MatchToken(TokenType.CloseParenthesis);

                return new ParenthesizedExpression(left, expression, right);
            }

            var numberToken = MatchToken(TokenType.Number);
            return new NumberLiteral(numberToken);
        }

        private SyntaxToken PeekToken(int offset)
        {
            var index = position + offset;   

            if (index < 0)
                return tokens[0];

            if (index >= tokens.Count)
                return tokens[tokens.Count - 1];

            return tokens[index];
        }

        private SyntaxToken Current => PeekToken(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken MatchToken(TokenType tokenType)
        {
            if (Current.TokenType == tokenType)
                return NextToken();

            AddError(Current, $"Unexpected token. Expected '{tokenType}' but was '{Current.TokenType}'");
            
            return new SyntaxToken(tokenType, Current.Position, null, null);
        }

        private void AddError(SyntaxToken token, string message)
        {
            errors.Add(new SyntaxError(token, message));
        }
    }
}