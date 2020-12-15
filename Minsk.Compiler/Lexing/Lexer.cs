using System;
using System.Collections.Generic;

namespace Minsk.Compiler.Lexing
{
    public sealed class Lexer
    {
        private readonly string text;
        private int position;
        private List<LexingError> errors = new List<LexingError>();

        public Lexer(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            this.text = text;
            position = 0;
        }

        public bool HasNext => position <= text.Length;

        public bool HasErrors => errors.Count > 0;

        public IEnumerable<LexingError> Errors => errors;

        public SyntaxToken NextToken()
        {
            var start = position;

            if (position > text.Length)
                throw new InvalidOperationException("EoF");

            if (position == text.Length)
            {
                Next();
                return new SyntaxToken(TokenType.EoF, start, string.Empty);
            }

            // <numbers> [0-9]+
            if (char.IsDigit(Current))
            {
                while(char.IsDigit(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                
                if (!int.TryParse(tokenText, out var value))
                    errors.Add(new LexingError(start, length, tokenText, "Unable to parse number to Int32"));

                return new SyntaxToken(TokenType.Number, start, tokenText, value);
            }

            // <whitespace>
            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                return new SyntaxToken(TokenType.Whitespace, start, tokenText);
            }

            // <operators> + - * ? ( )
            if (Current == '+')
                return new SyntaxToken(TokenType.Plus, position++, "+", null);
            if (Current == '-')
                return new SyntaxToken(TokenType.Minus, position++, "-", null);
            if (Current == '*')
                return new SyntaxToken(TokenType.Star, position++, "*", null);
            if (Current == '/')
                return new SyntaxToken(TokenType.ForwardSlash, position++, "/", null);
            if (Current == '(')
                return new SyntaxToken(TokenType.OpenParenthesis, position++, "(", null);
            if (Current == ')')
                return new SyntaxToken(TokenType.CloseParenthesis, position++, ")", null);

            {
                while (!char.IsWhiteSpace(Current) && Current != '\0')
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);

                errors.Add(new LexingError(start, length, tokenText, "Unexpected characters"));

                return new SyntaxToken(TokenType.Unknown, start, tokenText);
            }
        }

        private char Current => (position >= text.Length) ? '\0' : text[position];

        private char Peek => (position + 1 >= text.Length) ? '\0' : text[position + 1];

        private void Next()
        {
            position++;
        }
    }
}