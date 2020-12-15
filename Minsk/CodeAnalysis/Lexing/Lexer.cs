using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Lexing
{
    public sealed class Lexer
    {
        private readonly DiagnosticBag diagnostics;
        private readonly string text;
        private int position;

        public Lexer(DiagnosticBag diagnostics, string text)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            this.text = text;
            position = 0;
        }

        public bool HasNext => position <= text.Length;

        public LexToken NextToken()
        {
            var start = position;

            if (position > text.Length)
                throw new InvalidOperationException("EoF");

            if (position == text.Length)
            {
                Next();
                return new LexToken(TokenKind.EoF, start, string.Empty);
            }

            // <numbers> [0-9]+
            if (char.IsDigit(Current))
            {
                while(char.IsDigit(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                
                if (!int.TryParse(tokenText, out var value))
                    diagnostics.Lex.InvalidNumber(start, length, tokenText, $"Unable to parse '{tokenText}' to Int32");

                return new LexToken(TokenKind.Number, start, tokenText, value);
            }

            // <whitespace>
            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                return new LexToken(TokenKind.Whitespace, start, tokenText);
            }

            // true, false, keywords, identifiers
            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                var kind = SyntaxFacts.KeywordKind(tokenText);
                return new LexToken(kind, start, tokenText);
            }

            // <operators> + - * ? ( ) etc
            switch (Current)
            {
                case '+':
                    return new LexToken(TokenKind.Plus, position++, "+");
                case '-':
                    return new LexToken(TokenKind.Minus, position++, "-");
                case '*':
                    return new LexToken(TokenKind.Star, position++, "*");
                case '/':
                    return new LexToken(TokenKind.ForwardSlash, position++, "/");
                case '(':
                    return new LexToken(TokenKind.OpenParenthesis, position++, "(");
                case ')':
                    return new LexToken(TokenKind.CloseParenthesis, position++, ")");

                case '&':
                {
                    if (Peek(1) == '&')
                        return new LexToken(TokenKind.AmpersandAmperand, position += 2, "&&");
                    //return new SyntaxToken(TokenType.Ampersand, position++, "&");
                } break;

                case '!':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.BangEquals, position += 2, "!=");
                    return new LexToken(TokenKind.Bang, position++, "!");
                }

                case '=':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.EqualsEquals, position += 2, "==");
                } break;

                case '|':
                {
                    if (Peek(1) == '|')
                        return new LexToken(TokenKind.PipePipe, position += 2, "||");
                    //return new SyntaxToken(TokenType.Pipe, position++, "|");
                } break;
            }


            {
                while (!char.IsWhiteSpace(Current) && Current != '\0')
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);

                diagnostics.Lex.UnexpectedCharacters(start, length, tokenText, $"Unexpected characters '{tokenText}'");

                return new LexToken(TokenKind.Unknown, start, tokenText);
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