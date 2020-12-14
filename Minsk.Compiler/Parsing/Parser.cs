using System;
using System.Collections.Generic;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public class Parser
    {
        private List<SyntaxToken> tokens;
        private int position = 0;

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

            return new SyntaxToken(tokenType, Current.Position, null, null);
        }


        public ExpressionSyntaxNode Parse()
        {
            var primary = ParsePrimaryExpression();

            while (Current.TokenType == TokenType.Plus 
                || Current.TokenType == TokenType.Minus)
            {
                var left = primary;
                var operatorNode = ParseOperatorNode();
                var right = Parse();

                primary = new BinaryExpressionNode(left, operatorNode, right);
            }

            return primary;
        }

        public ExpressionSyntaxNode ParsePrimaryExpression()
        {
            var numberToken = Match(TokenType.Number);
            return new NumberSyntaxNode(numberToken);
        }

        public OperatorSyntaxNode ParseOperatorNode()
        {
            return Current.TokenType switch
            {
                TokenType.Plus  => new OperatorSyntaxNode(NextToken()),
                TokenType.Minus => new OperatorSyntaxNode(NextToken()),
                _               => throw new InvalidOperationException()
            };
        }
    }
}