namespace Minsk.CodeAnalysis.Lexing
{
    public enum TokenKind
    {
        // Special tokens
        Unknown,
        //Error,
        Whitespace,
        //EoL,
        EoF,
       
        // Operators
        Plus,
        Minus,
        Star,
        ForwardSlash,
        Bang,
        BangEquals,
        AmpersandAmperand,
        Equals,
        EqualsEquals,
        PipePipe,
        
        OpenParenthesis,
        CloseParenthesis,
        //OpenBrace,
        //CloseBrace,
        //OpenBracket,
        //CloseBracket,

        // Keywords
        TrueKeyword,
        FalseKeyword,
        
        // Literals
        Number,
        String,

        Identifier,
    }
}