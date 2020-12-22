using System.Collections.Generic;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(
            LabelSymbol label,
            BoundExpression condition,
            bool jumpIfTrue = false)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }

        public LabelSymbol Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }

        public override IEnumerable<BoundNode> Children
        { get { yield return Condition; } }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

        protected override string PrettyPrintText()
            => $"{Label.Name} when {JumpIfTrue}";
    }
}