namespace Minsk.CodeAnalysis.Parsing
{
    public abstract class Expression : SyntaxNode
    {
        protected Expression(SyntaxTree syntaxTree)
            : base(syntaxTree)
        { }
    }
}