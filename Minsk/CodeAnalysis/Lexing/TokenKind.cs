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
        [Description("&")]      Ampersand,
        [Description("&&")]     AmpersandAmperand,
        [Description("=")]      Equals,
        [Description("==")]     EqualsEquals,
        [Description("|")]      Pipe,
        [Description("||")]     PipePipe,
        [Description("<")]      Less,
        [Description("<=")]     LessOrEquals,
        [Description(">")]      Greater,
        [Description(">=")]     GreaterOrEquals,
        [Description("~")]      Tilde,
        [Description("^")]      Hat,

        [Description("(")]      OpenParenthesis,
        [Description(")")]      CloseParenthesis,
        [Description("{")]      OpenBrace,
        [Description("}")]      CloseBrace,
        //OpenBracket,
        //CloseBracket,
        [Description(",")]      Comma,
        [Description(":")]      Colon,
        [Description(";")]      SemiColon,

        // Keywords
        [Description("true")]   TrueKeyword,
        [Description("false")]  FalseKeyword,
        [Description("var")]    VarKeyword,
        [Description("let")]    LetKeyword,
        [Description("if")]     IfKeyword,
        [Description("else")]   ElseKeyword,
        [Description("while")]  WhileKeyword,
        [Description("for")]    ForKeyword,
        [Description("to")]     ToKeyword,

        // Literals
        Number,
        String,

        Identifier,
    }
}