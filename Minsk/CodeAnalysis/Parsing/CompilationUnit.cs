using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class CompilationUnit : SyntaxNode
    {
        public CompilationUnit(
            SyntaxTree syntaxTree,
            IEnumerable<MemberSyntax> members,
            LexToken endOfFileToken
        )
            : this(syntaxTree, members.ToImmutableArray(), endOfFileToken)
        { }

        public CompilationUnit(
            SyntaxTree syntaxTree,
            ImmutableArray<MemberSyntax> members,
            LexToken endOfFileToken
        )
            : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;

            foreach (var member in members)
                member.Parent = this;
        }

        public ImmutableArray<MemberSyntax> Members { get; }
        public LexToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children => Members;

        public override LexToken FirstToken => Members.First()?.FirstToken ?? EndOfFileToken;

        public override LexToken LastToken => EndOfFileToken;
    }
}
