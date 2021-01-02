using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraphBuilder
    {
        private AtomicBlock start;
        private AtomicBlock end;

        private Dictionary<BoundStatement, AtomicBlock> blockFromStatement
            = new Dictionary<BoundStatement, AtomicBlock>();

        private Dictionary<BoundLabel, AtomicBlock> blockFromLabel
            = new Dictionary<BoundLabel, AtomicBlock>();

        private List<Branch> branches = new List<Branch>();

        public static ControlFlowGraph Build(List<AtomicBlock> blocks)
        {
            var builder = new ControlFlowGraphBuilder();
            return builder.BuildInternal(blocks);
        }

        public ControlFlowGraph BuildInternal(List<AtomicBlock> blocks)
        {
            start = new AtomicBlock(isStart: true);
            end = new AtomicBlock(isEnd: true);

            if (!blocks.Any())
            {
                Connect(start, end);
            }
            else
            {
                Connect(start, blocks.First());
            }

            foreach (var block in blocks)
            {
                foreach (var statement in block.Statements)
                {
                    blockFromStatement.Add(statement, block);
                    if (statement is BoundLabelStatement labelStatement)
                        blockFromLabel.Add(labelStatement.Label, block);
                }
            }

            for (var i = 0; i < blocks.Count; i++)
            {
                var current = blocks[i];
                var next = i != blocks.Count - 1 ? blocks[i + 1] : end;

                foreach (var statement in current.Statements)
                {
                    var isLastStatementInBlock = statement == current.Statements.Last();

                    switch (statement.Kind)
                    {
                        case BoundNodeKind.ConditionalGotoStatement:
                            {
                                var cgt = statement as BoundConditionalGotoStatement;
                                var thenBlock = blockFromLabel[cgt.Label];
                                var elseBlock = next;
                                var negatedCondition = Negate(cgt.Condition);
                                var thenCondition = cgt.JumpIfTrue ? cgt.Condition : negatedCondition;
                                var elseCondition = cgt.JumpIfTrue ? negatedCondition : cgt.Condition;
                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                            }
                            break;

                        case BoundNodeKind.GotoStatement:
                            {
                                var gotoStatement = statement as BoundGotoStatement;
                                var toBlock = blockFromLabel[gotoStatement.Label];
                                Connect(current, toBlock);
                            }
                            break;

                        case BoundNodeKind.LabelStatement:
                            break;

                        case BoundNodeKind.ReturnStatement:
                            Connect(current, end);
                            break;

                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.VariableDeclarationStatement:
                            if (isLastStatementInBlock)
                                Connect(current, next);
                            break;

                        default:
                            throw new Exception($"{statement.Kind}");
                    }
                }
            }

            bool blockRemoved;
            do
            {
                blockRemoved = false;
                var deadBlock = blocks.FirstOrDefault(b => !b.Incoming.Any());

                if (deadBlock is not null)
                {
                    RemoveBlock(blocks, deadBlock);
                    blockRemoved = true;
                }
            } while (blockRemoved);

            blocks.Insert(0, start);
            blocks.Add(end);

            return new ControlFlowGraph(start, end, blocks, branches);
        }

        private void Connect(AtomicBlock from, AtomicBlock to, BoundExpression condition = null)
        {
            if (condition is BoundLiteralExpression literal)
            {
                var value = (bool)literal.Value;

                if (value)
                    condition = null;
                else
                    return;
            }

            var branch = new Branch(from, to, condition);
            branches.Add(branch);
            from.Outgoing.Add(branch);
            to.Incoming.Add(branch);
        }

        private void RemoveBlock(List<AtomicBlock> blocks, AtomicBlock block)
        {
            blocks.Remove(block);

            foreach (var incoming in block.Incoming)
            {
                incoming.From.Outgoing.Remove(incoming);
                branches.Remove(incoming);
            }

            foreach (var outgoing in block.Outgoing)
            {
                outgoing.To.Incoming.Remove(outgoing);
                branches.Remove(outgoing);
            }
        }

        private BoundExpression Negate(BoundExpression expression)
        {
            if (expression is BoundLiteralExpression literal)
            {
                var value = (bool)literal.Value;
                return new BoundLiteralExpression(!value);
            }

            var negate = BoundUnaryOperator.Bind(TokenKind.Bang, TypeSymbol.Bool);
            return new BoundUnaryExpression(negate, expression);
        }
    }
}
