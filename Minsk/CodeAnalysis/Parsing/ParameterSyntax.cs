﻿using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(LexToken identifier, TypeClauseSyntax type)
        {
            Identifier = identifier;
            Type = type;
        }

        public LexToken Identifier { get; }
        public TypeClauseSyntax Type { get; }

        public override string Text => Identifier.Text;

        public override LexToken FirstToken => Identifier;

        public override LexToken LastToken => Type.LastToken;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Type; } }

        public override SyntaxKind Kind => SyntaxKind.Parameter;
    }
}
