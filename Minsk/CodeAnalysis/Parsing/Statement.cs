namespace Minsk.CodeAnalysis.Parsing
{
    public abstract class Statement : SyntaxNode
    {
        protected Statement(SyntaxTree syntaxTree)
            : base(syntaxTree)
        { }
    }
}