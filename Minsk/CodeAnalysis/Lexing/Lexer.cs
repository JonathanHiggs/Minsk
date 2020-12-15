using System;

using Minsk.CodeAnalysis.Common;
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
                return new LexToken(TokenKind.EoF, start, 1, string.Empty);
            }

            // <numbers> [0-9]+
            if (char.IsDigit(Current))
            {
                while(char.IsDigit(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                
                if (!int.TryParse(tokenText, out var value))
                    diagnostics.Lex.InvalidNumber(
                        start, length, tokenText, $"Unable to parse '{tokenText}' to Int32");

                return new LexToken(TokenKind.Number, start, length, tokenText, value);
            }

            // <whitespace>
            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                return new LexToken(TokenKind.Whitespace, start, length, tokenText);
            }

            // true, false, keywords, identifiers
            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);
                var kind = SyntaxFacts.KeywordKind(tokenText);
                return new LexToken(kind, start, length, tokenText);
            }

            // <operators> + - * ? ( ) etc
            switch (Current)
            {
                case '+':
                    return new LexToken(TokenKind.Plus, Consume(1), "+");
                case '-':
                    return new LexToken(TokenKind.Minus, Consume(1), "-");
                case '*':
                    return new LexToken(TokenKind.Star, Consume(1), "*");
                case '/':
                    return new LexToken(TokenKind.ForwardSlash, Consume(1), "/");
                case '(':
                    return new LexToken(TokenKind.OpenParenthesis, Consume(1), "(");
                case ')':
                    return new LexToken(TokenKind.CloseParenthesis, Consume(1), ")");

                case '&':
                {
                    if (Peek(1) == '&')
                        return new LexToken(TokenKind.AmpersandAmperand, Consume(2), "&&");
                    //return new SyntaxToken(TokenType.Ampersand, Consume(1), "&");
                } break;

                case '!':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.BangEquals, Consume(2), "!=");
                    return new LexToken(TokenKind.Bang, Consume(1), "!");
                }

                case '=':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.EqualsEquals, Consume(2), "==");
                } break;

                case '|':
                {
                    if (Peek(1) == '|')
                        return new LexToken(TokenKind.PipePipe, Consume(2), "||");
                    //return new SyntaxToken(TokenType.Pipe, Consume(1), "|");
                } break;
            }


            {
                // Note: Might want to spit out single characters rather than clobber everything
                while (!char.IsWhiteSpace(Current) && Current != '\0')
                    Next();

                var length = position - start;
                var tokenText = text.Substring(start, length);

                diagnostics.Lex.InvalidCharacters(
                    start, length, tokenText, $"Unexpected characters '{tokenText}'");

                return new LexToken(TokenKind.Unknown, start, length, tokenText);
            }
        }

        private char Current => (position >= text.Length) ? '\0' : text[position];

        private char Peek(int offset) 
            => (position + offset >= text.Length) ? '\0' : text[position + offset];

        private void Next()
        {
            position++;
        }

        private TextSpan Consume(int length)
        {
            var start = position;
            position += length;
            return new TextSpan(start, length);
        }    
    }
}