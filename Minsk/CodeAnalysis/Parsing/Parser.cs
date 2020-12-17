using System;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class Parser
    {
        private readonly SourceText source;
        private readonly DiagnosticBag diagnostics;
        private readonly ImmutableArray<LexToken> tokens;

        private int position = 0;

        public Parser(SourceText source, DiagnosticBag diagnostics)
        {
            this.source = source
                ?? throw new ArgumentNullException(nameof(source));

            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            tokens = Lexer.Lex(source, diagnostics)
                .Where(t => t.Kind != TokenKind.Whitespace)
                .ToImmutableArray();
        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokenKind.EoF);
            return new SyntaxTree(source, expression, eof, diagnostics);
        }

        private Expression ParseExpression()
        {
            return (Current, Next) switch
            {
                (TokenKind.Identifier, TokenKind.Equals)
                    => ParseAssignmentExpression(),

                //(TokenKind c, _) when c.IsUnaryOperator() => ParseUnaryOperator()

                _ => ParsePrecedenceExpression()
            };
        }

        private Expression ParseAssignmentExpression()
        {
            var identifierToken = MatchToken(TokenKind.Identifier);
            var equalsToken = MatchToken(TokenKind.Equals);
            var expression = ParseExpression();
            return new AssignmentExpression(identifierToken, equalsToken, expression);
        }

        private Expression ParsePrecedenceExpression(int parentPrecedence = 0)
        {
            // ToDo: this could be cleaned up?
            Expression left;
            var unaryPrecedence = Current.UnaryOperatorPrecedence();

            if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
                left = ParseUnaryExpression(unaryPrecedence);
            else
                left = ParsePrimaryExpression();

            // ToDo: extract ParseBinaryExpression?
            while (true)
            {
                var binaryPrecendence = Current.BinaryOperatorPrecedence();
                if (binaryPrecendence == 0 || binaryPrecendence <= parentPrecedence)
                    break;

                var operatorToken = MatchBinaryOperatorToken();
                var right = ParsePrecedenceExpression(binaryPrecendence);

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParseUnaryExpression(int parentPrecedence = 0)
        {
            var operatorToken = MatchUnaryOperatorToken();
            var operand = ParsePrecedenceExpression(parentPrecedence);
            return new UnaryExpression(operatorToken, operand);
        }

        private Expression ParsePrimaryExpression()
        {
            return Current switch
            {
                TokenKind.OpenParenthesis
                    => ParseParenthesisExpression(),

                TokenKind.TrueKeyword or TokenKind.FalseKeyword
                    => ParseBooleanLiteral(),

                TokenKind.Identifier
                    => ParseNameExpression(),

                TokenKind.Number
                    => ParseNumberLiteral(),

                _ => throw new Exception()
            };
        }

        private Expression ParseParenthesisExpression()
        {
            var left = NextToken();
            var expression = ParseExpression();
            var right = MatchToken(TokenKind.CloseParenthesis);
            return new ParenthesizedExpression(left, expression, right);
        }

        private Expression ParseBooleanLiteral()
        {
            var isTrue = Current == TokenKind.TrueKeyword;
            var token = isTrue
                ? MatchToken(TokenKind.TrueKeyword)
                : MatchToken(TokenKind.FalseKeyword);

            return new LiteralExpression(token, isTrue);
        }

        private Expression ParseNameExpression()
        {
            var identifierToke = MatchToken(TokenKind.Identifier);
            return new NameExpression(identifierToke);
        }

        private Expression ParseNumberLiteral()
        {
            var numberToken = MatchToken(TokenKind.Number);
            return new LiteralExpression(numberToken);
        }

        private LexToken PeekToken(int offset)
        {
            var index = position + offset;

            if (index < 0)
                return tokens[0];

            if (index >= tokens.Length)
                return tokens[tokens.Length - 1];

            return tokens[index];
        }

        private TokenKind Current => PeekToken(0).Kind;

        private TokenKind Next => PeekToken(1).Kind;

        private LexToken MatchToken(TokenKind tokenKind)
        {
            if (Current == tokenKind)
                return NextToken();

            diagnostics.Syntax.UnexpectedToken(
                PeekToken(0),
                $"Expected '{tokenKind}' but was '{PeekToken(0).Text}'");

            return new LexToken(tokenKind, PeekToken(0).Span, null, null);
        }

        private LexToken MatchBinaryOperatorToken()
        {
            if (Current.IsBinaryOperator())
                return NextToken();

            diagnostics.Syntax.UnexpectedToken(
                PeekToken(0),
                $"Expected binary operator but was '{PeekToken(0).Text}'");

            return new LexToken(TokenKind.Plus, PeekToken(0).Span, null, null);
        }

        private LexToken MatchUnaryOperatorToken()
        {
            if (Current.IsUnaryOperator())
                return NextToken();

            diagnostics.Syntax.UnexpectedToken(
                PeekToken(0),
                $"Expected unary operator but was '{PeekToken(0).Text}'");

            return new LexToken(TokenKind.Plus, PeekToken(0).Span, null, null);
        }

        private LexToken NextToken()
        {
            var current = PeekToken(0);
            position++;
            return current;
        }
    }
}