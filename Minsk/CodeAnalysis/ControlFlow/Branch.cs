using System.IO;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Branch
    {
        public Branch(AtomicBlock from, AtomicBlock to, BoundExpression condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }

        public AtomicBlock From { get; }
        public AtomicBlock To { get; }
        public BoundExpression Condition { get; }

        public override string ToString()
        {
            if (Condition is null)
                return string.Empty;

            using var stringWriter = new StringWriter();

            Condition.WriteTo(stringWriter);

            return stringWriter.ToString();
        }
    }
}
