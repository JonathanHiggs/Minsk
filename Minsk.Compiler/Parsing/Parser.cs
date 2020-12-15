using System;
using System.Collections.Generic;

using Minsk.Compiler.Core;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public class Parser
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
            var eof = Match(TokenType.EoF);
            return new SyntaxTree(expression, eof, errors);
        }

        private Expression ParseExpression()
        {
            return ParseTermExpression();
        }

        private Expression ParseTermExpression()
        {
            var primary = ParseFactorExpression();

            while (Current.TokenType == TokenType.Plus 
                || Current.TokenType == TokenType.Minus)
            {
                var left = primary;
                var operatorNode = ParseOperatorNode();
                var right = ParseFactorExpression();

                primary = new BinaryExpression(left, operatorNode, right);
            }

            return primary;
        }

        private Expression ParseFactorExpression()
        {
            var primary = ParsePrimaryExpression();

            while (Current.TokenType == TokenType.Star
                || Current.TokenType == TokenType.ForwardSlash)
            {
                var left = primary;
                var operatorNode = ParseOperatorNode();
                var right = ParsePrimaryExpression();

                primary = new BinaryExpression(left, operatorNode, right);
            }

            return primary;
        }

        private Expression ParsePrimaryExpression()
        {
            if (Current.TokenType == TokenType.OpenParenthesis)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = Match(TokenType.CloseParenthesis);

                return new ParenthesizedExpression(left, expression, right);
            }

            var numberToken = Match(TokenType.Number);
            return new NumberLiteral(numberToken);
        }

        private OperatorNode ParseOperatorNode()
        {
            var token = NextToken();

            if (!token.TokenType.IsOperator())
                errors.Add(new SyntaxError(token, "Operator required"));

            return new OperatorNode(token);
        }

        private SyntaxToken Peek(int offset)
        {
            var index = position + offset;   

            if (index < 0)
                return tokens[0];

            if (index >= tokens.Count)
                return tokens[tokens.Count - 1];

            return tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken Match(TokenType tokenType)
        {
            if (Current.TokenType == tokenType)
                return NextToken();

            errors.Add(new SyntaxError(Current, $"Unexpected token. Expected '{tokenType}' but was '{Current.TokenType}'"));
            return new SyntaxToken(tokenType, Current.Position, null, null);
        }
    }
}