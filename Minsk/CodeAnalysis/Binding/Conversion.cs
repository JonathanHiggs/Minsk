using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Conversion
    {
        public static readonly Conversion None = new Conversion(exists: false);
        public static readonly Conversion Identity = new Conversion(isIdentity: true);
        public static readonly Conversion Implicit = new Conversion(isImplicit: true);
        public static readonly Conversion Explicit = new Conversion(isExplicit: true);

        private Conversion(
            bool exists = true,
            bool isIdentity = false,
            bool isImplicit = false,
            bool isExplicit = false)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
            IsExplicit = isExplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit { get; }


        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Identity;

            if (from == TypeSymbol.Bool || from == TypeSymbol.Int)
            {
                if (to == TypeSymbol.String)
                    return Explicit;
            }
            else if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Int || to == TypeSymbol.Bool)
                    return Explicit;
            }

            return None;
        }
    }
}
