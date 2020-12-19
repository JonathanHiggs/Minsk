using System;
using System.ComponentModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Lexing
{
    public static class TokenKindExtensions
    {
        public static bool HasDefaultText(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.EoF:
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Star:
                case TokenKind.ForwardSlash:
                case TokenKind.Bang:
                case TokenKind.BangEquals:
                case TokenKind.AmpersandAmperand:
                case TokenKind.Equals:
                case TokenKind.EqualsEquals:
                case TokenKind.PipePipe:
                case TokenKind.OpenParenthesis:
                case TokenKind.CloseParenthesis:
                case TokenKind.OpenBrace:
                case TokenKind.CloseBrace:
                case TokenKind.Less:
                case TokenKind.LessOrEquals:
                case TokenKind.Greater:
                case TokenKind.GreaterOrEquals:
                    return true;

                case TokenKind.TrueKeyword:
                case TokenKind.FalseKeyword:
                case TokenKind.VarKeyword:
                case TokenKind.LetKeyword:
                case TokenKind.IfKeyword:
                case TokenKind.ElseKeyword:
                case TokenKind.WhileKeyword:
                    return true;

                default:
#if DEBUG
                    // Extra check on the codebase
                    var attr = typeof(TokenKind).GetMember(kind.ToString())
                    .FirstOrDefault(m => m.DeclaringType == typeof(TokenKind))
                    .GetCustomAttribute<DescriptionAttribute>();

                    Debug.Assert(attr is null, $"TokenKind.{kind} should have HasDefaultText == true");
#endif

                    return false;
            }
        }
        public static string GetText(this TokenKind kind)
        {
            // ToDo: SourceCodeGenerators would be better for this

            if (!kind.HasDefaultText())
            {
#if DEBUG
                var attr = typeof(TokenKind).GetMember(kind.ToString())
                    .FirstOrDefault(m => m.DeclaringType == typeof(TokenKind))
                    .GetCustomAttribute<DescriptionAttribute>();

                Debug.Assert(attr is null, $"TokenKind.{kind} should have HasDefaultText == true");
#endif
                return null;
            }

            var attribute = typeof(TokenKind).GetMember(kind.ToString())
                .FirstOrDefault(m => m.DeclaringType == typeof(TokenKind))
                .GetCustomAttribute<DescriptionAttribute>();

            if (attribute is null)
                throw new Exception(
                    $"TokenKind.{kind} should have a Description attribute with the default text");

            return attribute.Description;
        }
    }
}