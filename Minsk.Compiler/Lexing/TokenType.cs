namespace Minsk.Compiler.Lexing
{
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
        
        // Literals
        Number,
        String,
    }
}