using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public abstract class Diagnostic
    {
        protected Diagnostic(
            TextLocation location,
            string message,
            DiagnosticWarningLevel level = DiagnosticWarningLevel.Error)
        {
            Location = location;
            Message = message ?? string.Empty;
            Level = level;
        }

        public abstract DiagnosticKind Kind { get; }

        public TextLocation Location { get; }
        public string Message { get; }
        public DiagnosticWarningLevel Level { get; }

        public bool IsInfo          => Level == DiagnosticWarningLevel.Info;
        public bool IsNotInfo       => !IsInfo;
        public bool IsWarning       => Level == DiagnosticWarningLevel.Warn;
        public bool IsNotWarning    => !IsWarning;
        public bool IsError         => Level == DiagnosticWarningLevel.Error;
        public bool IsNotError      => !IsError;
    }
}