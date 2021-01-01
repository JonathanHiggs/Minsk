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
        [Description("\0")]         EoF,

        // Operators
        [Description("+")]          Plus,
        [Description("-")]          Minus,
        [Description("*")]          Star,
        [Description("/")]          ForwardSlash,
        [Description("!")]          Bang,
        [Description("!=")]         BangEquals,
        [Description("&")]          Ampersand,
        [Description("&&")]         AmpersandAmperand,
        [Description("=")]          Equals,
        [Description("==")]         EqualsEquals,
        [Description("|")]          Pipe,
        [Description("||")]         PipePipe,
        [Description("<")]          Less,
        [Description("<=")]         LessOrEquals,
        [Description(">")]          Greater,
        [Description(">=")]         GreaterOrEquals,
        [Description("~")]          Tilde,
        [Description("^")]          Hat,

        [Description("(")]          OpenParenthesis,
        [Description(")")]          CloseParenthesis,
        [Description("{")]          OpenBrace,
        [Description("}")]          CloseBrace,
        //OpenBracket,
        //CloseBracket,
        [Description(",")]          Comma,
        [Description(":")]          Colon,
        [Description(";")]          SemiColon,

        // Keywords
        [Description("break")]      BreakKeyword,
        [Description("continue")]   ContinueKeyword,
        [Description("else")]       ElseKeyword,
        [Description("false")]      FalseKeyword,
        [Description("for")]        ForKeyword,
        [Description("function")]   FunctionKeyword,
        [Description("if")]         IfKeyword,
        [Description("let")]        LetKeyword,
        [Description("to")]         ToKeyword,
        [Description("true")]       TrueKeyword,
        [Description("var")]        VarKeyword,
        [Description("while")]      WhileKeyword,

        // Literals
        Number,
        String,

        Identifier,
    }
}