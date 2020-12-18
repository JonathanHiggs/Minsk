using System.ComponentModel;

namespace Minsk.CodeAnalysis.Lexing
{
    public enum TokenKind
    {
        // Special tokens
        Unknown,
        //Error,
        Whitespace,
        //EoL,
        [Description("\0")]     EoF,

        // Operators
        [Description("+")]      Plus,
        [Description("-")]      Minus,
        [Description("*")]      Star,
        [Description("/")]      ForwardSlash,
        [Description("!")]      Bang,
        [Description("!=")]     BangEquals,
        [Description("&&")]     AmpersandAmperand,
        [Description("=")]      Equals,
        [Description("==")]     EqualsEquals,
        [Description("||")]     PipePipe,

        [Description("(")]      OpenParenthesis,
        [Description(")")]      CloseParenthesis,
        [Description("{")]      OpenBrace,
        [Description("}")]      CloseBrace,
        //OpenBracket,
        //CloseBracket,

        // Keywords
        [Description("true")]   TrueKeyword,
        [Description("false")]  FalseKeyword,
        [Description("var")]    VarKeyword,
        [Description("let")]    LetKeyword,

        // Literals
        Number,
        String,

        Identifier,
    }
}