using System;
using System.Collections.Generic;
using System.Text;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    public sealed class Lexer
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SourceText source;

        private LexCursor cursor = new LexCursor();
        private int position => cursor.End;

        public Lexer(SourceText source, DiagnosticBag diagnostics)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            this.source = source;
        }

        public static IEnumerable<LexToken> Lex(SourceText source, DiagnosticBag diagnostics = null)
        {
            diagnostics ??= new DiagnosticBag();
            var lexer = new Lexer(source, diagnostics);

            // ToDo: track previous token and emit diagnostic warning when ambiguous tokens together
            // eg. =+= or =!=
            while (lexer.HasNext)
                yield return lexer.NextToken();
        }

        public bool HasNext => cursor.End <= source.Length;

        public LexToken NextToken()
        {
            if (position > source.Length)
                throw new InvalidOperationException("EoF");

            if (position == source.Length)
                return EmitToken(TokenKind.EoF, 1);

            switch (Current)
            {
                case '\0':  return ReadNullTerminator();

                case '+':   return EmitToken(TokenKind.Plus, 1);
                case '-':   return EmitToken(TokenKind.Minus, 1);
                case '*':   return EmitToken(TokenKind.Star, 1);
                case '/':   return EmitToken(TokenKind.ForwardSlash, 1);
                case '(':   return EmitToken(TokenKind.OpenParenthesis, 1);
                case ')':   return EmitToken(TokenKind.CloseParenthesis, 1);
                case '{':   return EmitToken(TokenKind.OpenBrace, 1);
                case '}':   return EmitToken(TokenKind.CloseBrace, 1);

                case '"':
                    return ReadStringToken();

                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    return ReadNumberToken();

                case ' ': case '\t': case '\r': case '\n':
                    return ReadWhitespaceToken();

                case '&':
                {
                    if (Next == '&')
                        return EmitToken(TokenKind.AmpersandAmperand, 2);
                    return EmitToken(TokenKind.Ampersand, 1);
                }

                case '|':
                {
                    if (Next == '|')
                        return EmitToken(TokenKind.PipePipe, 2);
                    return EmitToken(TokenKind.Pipe, 1);
                }

                case '^':
                    return EmitToken(TokenKind.Hat, 1);

                case '~':
                    return EmitToken(TokenKind.Tilde, 1);

                case '!':
                {
                    if (Next == '=')
                        return EmitToken(TokenKind.BangEquals, 2);
                    return EmitToken(TokenKind.Bang, 1);
                }

                case '=':
                {
                    if (Next == '=')
                        return EmitToken(TokenKind.EqualsEquals, 2);
                    return EmitToken(TokenKind.Equals, 1);
                }

                case '<':
                {
                    if (Next == '=')
                        return EmitToken(TokenKind.LessOrEquals, 2);
                    return EmitToken(TokenKind.Less, 1);
                }
                case '>':
                {
                    if (Next == '=')
                        return EmitToken(TokenKind.GreaterOrEquals, 2);
                    return EmitToken(TokenKind.Greater, 1);
                }

                default:
                {
                    if (char.IsLetter(Current))
                        return ReadKeywordOrIdentifier();
                    if (char.IsWhiteSpace(Current))
                        return ReadWhitespaceToken();
                } break;
            }

            return ReadUnknownToken();
        }

        private LexToken ReadNullTerminator()
        {
            cursor.Advance(source.Length - cursor.End);
            diagnostics.Lex.UnexpectedNullTerminator(cursor, CurrentText);
            return EmitToken(TokenKind.EoF);
        }

        private LexToken ReadStringToken()
        {
            cursor.Advance();
            var sb = new StringBuilder();

            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        diagnostics.Lex.UnterminatedString(TextSpan.From(cursor.Start, 1), CurrentText);
                        done = true;
                        break;

                    case '"':
                    {
                        if (Next == '"')
                        {
                            sb.Append('"');
                            cursor.Advance(2);
                        }
                        else
                        {
                            cursor.Advance();
                            done = true;
                        }
                    } break;

                    default:
                        sb.Append(Current);
                        cursor.Advance();
                        break;
                }
            }

            return EmitValueToken(TokenKind.String, sb.ToString());
        }

        private LexToken ReadNumberToken()
        {
            while (char.IsDigit(Current))
                cursor.Advance();

            if (!int.TryParse(CurrentText, out var value))
                diagnostics.Lex.InvalidNumber(
                    cursor, CurrentText, $"Unable to parse '{CurrentText}' to Int32");

            return EmitValueToken(TokenKind.Number, value);
        }

        private LexToken ReadWhitespaceToken()
        {
            while (char.IsWhiteSpace(Current))
                cursor.Advance();

            return EmitToken(TokenKind.Whitespace);
        }

        private LexToken ReadKeywordOrIdentifier()
        {
            while (char.IsLetter(Current))
                cursor.Advance();

            return EmitValueToken(SyntaxFacts.KeywordOrIdentifierKind(CurrentText), CurrentText);
        }

        private LexToken ReadUnknownToken()
        {
            // Note: Might want to spit out single characters rather than clobber everything
            while (!char.IsWhiteSpace(Current) && Current != '\0')
                cursor.Advance();

            diagnostics.Lex.InvalidCharacters(
                cursor, CurrentText, $"Unexpected characters '{CurrentText}'");

            return EmitToken(TokenKind.Unknown);
        }

        private LexToken EmitToken(TokenKind kind, int numberOfCharsConsumed = 0)
        {
            cursor.Advance(numberOfCharsConsumed);
            var tokenText = CurrentText;
            return new LexToken(kind, Consume(), tokenText);
        }

        private LexToken EmitValueToken(TokenKind kind, object value, int numberOfCharsConsumed = 0)
        {
            var tokenText = CurrentText;
            return new LexToken(kind, Consume(numberOfCharsConsumed), tokenText, value);
        }

        private TextSpan Consume(int numberOfChars = 0)
        {
            // ToDo: remove when LexCursor takes in the SourceText
            var span = cursor.Consume(numberOfChars);
            if (span.End > source.Length)
                span = new TextSpan(span.Start, source.Length - span.Start);
            return span;
        }

        private char Current => (position >= source.Length) ? '\0' : source[position];

        private char Next => Peek(1);

        private string CurrentText
            => cursor.Start + cursor.Length <= source.Length
            ? source.ToString(cursor.Start, cursor.Length)
            : string.Empty;

        private char Peek(int offset)
            => (position + offset >= source.Length) ? '\0' : source[position + offset];
    }
}