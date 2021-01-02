using System.Collections.Generic;
using System.IO;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class AtomicBlock
    {
        public AtomicBlock()
        { }

        public AtomicBlock(bool isStart = false, bool isEnd = false)
        {
            IsStart = isStart;
            IsEnd = isEnd;
        }

        public bool IsStart { get; }
        public bool IsEnd { get; }
        public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
        public List<Branch> Incoming { get; } = new List<Branch>();
        public List<Branch> Outgoing { get; } = new List<Branch>();

        public override string ToString()
        {
            if (IsStart)
                return "<Start>";

            if (IsEnd)
                return "<End>";

            using var stringWriter = new StringWriter();

            foreach (var statement in Statements)
            {
                statement.WriteTo(stringWriter);
            }

            return stringWriter.ToString();
        }
    }
}
