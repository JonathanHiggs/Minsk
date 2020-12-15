namespace Minsk.Compiler.Lexing
{
    // ToDo: rename to TokenKind
    public enum TokenType
    {
        // Special tokens
        Unknown,
        Error,
        Whitespace,
        EoL,
        EoF,
       
        // Operators
        Plus,
        Minus,
        Star,
        ForwardSlash,
        OpenParenthesis,
        CloseParenthesis,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,

        // Keywords
        TrueKeyword,
        FalseKeyword,
        
        // Literals
        Number,
        String,

        Identifier
    }
}