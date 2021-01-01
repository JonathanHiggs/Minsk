using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class FunctionDeclaration : MemberSyntax
    {
        public FunctionDeclaration(
            LexToken functionKeyword,
            LexToken identifier,
            LexToken openParentheses,
            SeparatedSyntaxList<ParameterSyntax> parameters,
            LexToken closeParentheses,
            TypeClauseSyntax typeClause,
            Statement body)
        {
            FunctionKeyword = functionKeyword;
            Identifier = identifier;
            OpenParentheses = openParentheses;
            Parameters = parameters;
            CloseParentheses = closeParentheses;
            TypeClause = typeClause;
            Body = body;
        }

        public LexToken FunctionKeyword { get; }
        public LexToken Identifier { get; }
        public LexToken OpenParentheses { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public LexToken CloseParentheses { get; }
        public TypeClauseSyntax TypeClause { get; }
        public Statement Body { get; }

        public override string Text => Identifier.Text;

        public override LexToken FirstToken => FunctionKeyword;

        public override LexToken LastToken => Body.LastToken;

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                foreach (var parameter in Parameters)
                    yield return parameter;
                yield return Body;
            }
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
    }
}
