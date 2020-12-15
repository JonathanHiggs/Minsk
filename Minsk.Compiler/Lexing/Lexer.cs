using System;
using System.Collections.Generic;
using Minsk.Compiler.Parsing;

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

            // true, false, keywords, identifiers
            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                var kind = SyntaxFacts.KeywordKind(tokenText);
                return new SyntaxToken(kind, start, tokenText);
            }

            // <operators> + - * ? ( ) etc
            switch (Current)
            {
                case '+':
                    return new SyntaxToken(TokenType.Plus, position++, "+");
                case '-':
                    return new SyntaxToken(TokenType.Minus, position++, "-");
                case '*':
                    return new SyntaxToken(TokenType.Star, position++, "*");
                case '/':
                    return new SyntaxToken(TokenType.ForwardSlash, position++, "/");
                case '(':
                    return new SyntaxToken(TokenType.OpenParenthesis, position++, "(");
                case ')':
                    return new SyntaxToken(TokenType.CloseParenthesis, position++, ")");

                case '&':
                {
                    if (Peek(1) == '&')
                        return new SyntaxToken(TokenType.AmpersandAmperand, position += 2, "&&");
                    //return new SyntaxToken(TokenType.Ampersand, position++, "&");
                } break;

                case '!':
                {
                    if (Peek(1) == '=')
                        return new SyntaxToken(TokenType.BangEquals, position += 2, "!=");
                    return new SyntaxToken(TokenType.Bang, position++, "!");
                }

                case '=':
                {
                    if (Peek(1) == '=')
                        return new SyntaxToken(TokenType.EqualsEquals, position += 2, "==");
                } break;

                case '|':
                {
                    if (Peek(1) == '|')
                        return new SyntaxToken(TokenType.PipePipe, position += 2, "||");
                    //return new SyntaxToken(TokenType.Pipe, position++, "|");
                } break;
            }


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

        private char Peek(int offset) 
            => (position + offset >= text.Length) ? '\0' : text[position + offset];

        private void Next()
        {
            position++;
        }
    }
}