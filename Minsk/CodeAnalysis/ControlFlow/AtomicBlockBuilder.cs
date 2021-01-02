using System;
using System.Collections.Generic;
using System.Linq;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class AtomicBlockBuilder
    {
        private List<BoundStatement> statements = new List<BoundStatement>();
        private List<AtomicBlock> blocks = new List<AtomicBlock>();

        public static List<AtomicBlock> Build(BoundBlockStatement block)
        {
            var builder = new AtomicBlockBuilder();
            return builder.BuildInternal(block);
        }

        private List<AtomicBlock> BuildInternal(BoundBlockStatement block)
        {
            foreach (var statement in block.Statements)
            {
                switch (statement.Kind)
                {
                    case BoundNodeKind.ConditionalGotoStatement:
                    case BoundNodeKind.GotoStatement:
                    case BoundNodeKind.ReturnStatement:
                        statements.Add(statement);
                        StartBlock();
                        break;

                    case BoundNodeKind.ExpressionStatement:
                    case BoundNodeKind.VariableDeclarationStatement:
                        statements.Add(statement);
                        break;

                    case BoundNodeKind.LabelStatement:
                        StartBlock();
                        statements.Add(statement);
                        break;

                    case BoundNodeKind.ConditionalStatement:
                    case BoundNodeKind.ForToStatement:
                    case BoundNodeKind.WhileStatement:
                        throw new Exception($"Statement kind '{statement.Kind}' should be lowered out already");

                    case BoundNodeKind.ErrorStatement:
                    case BoundNodeKind.BlockStatement:
                    default:
                        throw new Exception($"Unexpected statement kind '{statement.Kind}'");
                }
            }

            EndBlock();

            return blocks;
        }

        private void StartBlock()
        {
            EndBlock();
        }

        private void EndBlock()
        {
            if (statements.Any())
            {
                var block = new AtomicBlock();
                block.Statements.AddRange(statements);
                blocks.Add(block);
                statements.Clear();
            }
        }
    }
}
