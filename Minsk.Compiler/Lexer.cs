using System;


namespace Minsk.Compiler
{
    public class Lexer
    {
        private readonly string text;
        private int position;

        public Lexer(string text)
        {
            this.text = text;
            position = 0;
        }

        private char Current => (position >= text.Length) ? '\0' : text[position];

        private char Peek => (position + 1 >= text.Length) ? '\0' : text[position + 1];

        private void Next()
        {
            position++;
        }

        public bool HasNext => position <= text.Length;

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
                int.TryParse(tokenText, out var value);
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
                return new SyntaxToken(TokenType.Unknown, start, tokenText);
            }
        }
    }
}