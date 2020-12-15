using System;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Lexing
{
    public sealed class Lexer
    {
        private readonly DiagnosticBag diagnostics;
        private readonly string text;

        private LexCursor cursor = new LexCursor();
        private int position => cursor.End;

        public Lexer(DiagnosticBag diagnostics, string text)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            this.text = text;
        }

        public bool HasNext => cursor.End <= text.Length;

        public LexToken NextToken()
        {
            if (position > text.Length)
                throw new InvalidOperationException("EoF");

            if (position == text.Length)
                return new LexToken(TokenKind.EoF, cursor.Consume(1), string.Empty);

            // <numbers> [0-9]+
            if (char.IsDigit(Current))
            {
                while(char.IsDigit(Current))
                    cursor.Advance();

                var tokenText = CurrentText;
                
                if (!int.TryParse(tokenText, out var value))
                    diagnostics.Lex.InvalidNumber(
                        cursor, tokenText, $"Unable to parse '{tokenText}' to Int32");

                return new LexToken(TokenKind.Number, cursor.Consume(), tokenText, value);
            }

            // <whitespace>
            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    cursor.Advance();

                var tokenText = CurrentText;
                return new LexToken(TokenKind.Whitespace, cursor.Consume(), tokenText);
            }

            // true, false, keywords, identifiers
            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                    cursor.Advance();

                var tokenText = CurrentText;
                var kind = SyntaxFacts.KeywordKind(tokenText);
                return new LexToken(kind, cursor.Consume(), tokenText);
            }

            // <operators> + - * ? ( ) etc
            switch (Current)
            {
                case '+':
                    return new LexToken(TokenKind.Plus, cursor.Consume(1), "+");
                case '-':
                    return new LexToken(TokenKind.Minus, cursor.Consume(1), "-");
                case '*':
                    return new LexToken(TokenKind.Star, cursor.Consume(1), "*");
                case '/':
                    return new LexToken(TokenKind.ForwardSlash, cursor.Consume(1), "/");
                case '(':
                    return new LexToken(TokenKind.OpenParenthesis, cursor.Consume(1), "(");
                case ')':
                    return new LexToken(TokenKind.CloseParenthesis, cursor.Consume(1), ")");

                case '&':
                {
                    if (Peek(1) == '&')
                        return new LexToken(TokenKind.AmpersandAmperand, cursor.Consume(2), "&&");
                    //return new SyntaxToken(TokenType.Ampersand, cursor.Consume(1), "&");
                } break;

                case '!':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.BangEquals, cursor.Consume(2), "!=");
                    return new LexToken(TokenKind.Bang, cursor.Consume(1), "!");
                }

                case '=':
                {
                    if (Peek(1) == '=')
                        return new LexToken(TokenKind.EqualsEquals, cursor.Consume(2), "==");
                } break;

                case '|':
                {
                    if (Peek(1) == '|')
                        return new LexToken(TokenKind.PipePipe, cursor.Consume(2), "||");
                    //return new SyntaxToken(TokenType.Pipe, cursor.Consume(1), "|");
                } break;
            }


            {
                // Note: Might want to spit out single characters rather than clobber everything
                while (!char.IsWhiteSpace(Current) && Current != '\0')
                    cursor.Advance();

                var tokenText = CurrentText;

                diagnostics.Lex.InvalidCharacters(
                    cursor, tokenText, $"Unexpected characters '{tokenText}'");

                return new LexToken(TokenKind.Unknown, cursor.Consume(), tokenText);
            }
        }

        private char Current => (position >= text.Length) ? '\0' : text[position];

        private string CurrentText
            => text.Substring(cursor.Start, cursor.Length);

        private char Peek(int offset) 
            => (position + offset >= text.Length) ? '\0' : text[position + offset];
    }
}