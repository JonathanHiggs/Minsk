using System;
using System.Collections.Generic;

namespace Minsk.Compiler
{
    public static class NodeTreePrinter
    {
        class NodeInfo
        {
            public SyntaxNode Node;
            public string Text;
            public int Size => Text.Length;
            public int StartPosition;
            public int EndPosition
            {
                get => StartPosition + Size;
                set => StartPosition = value - Size;
            }
            public NodeInfo Parent, Left, Right;
        }

        public static void Print(this SyntaxNode root, int topMargin = 2, int leftMargin = 2)
        {
            if (root is null)
                return;

            int rootTop = Console.CursorTop + topMargin;
            var last  = new List<NodeInfo>();
            var next = root;
            for (var level = 0; next != null; level++)
            {
                var item = new NodeInfo { Node = next, Text = next.Text };

                if (level < last.Count)
                {
                    item.StartPosition = last[level].EndPosition + 1;
                    last[level] = item;
                }
                else
                {
                    item.StartPosition = leftMargin;
                    last.Add(item);
                }

                if (level > 0)
                {
                    item.Parent = last[level - 1];

                    if (item.Parent.Node is BinaryExpressionNode binary)
                    {
                        if (next == binary.Left)
                        {
                            item.Parent.Left = item;
                            item.EndPosition = Math.Max(item.EndPosition, item.Parent.StartPosition);
                        }
                        else
                        {
                            item.Parent.Right = item;
                            item.StartPosition = Math.Max(item.StartPosition, item.Parent.EndPosition);
                        }
                    }
                    else if (item.Parent.Node is NumberSyntaxNode number || item.Parent.Node is OperatorSyntaxNode)
                    { 
                        // Shouldn't get here
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                if (next is BinaryExpressionNode b)
                {
                    next = b.Left ?? b.Right;

                    for (; next == null; item = item.Parent)
                    {
                        Print(item, rootTop + 2 * level);

                        if (--level < 0)
                            break;

                        if (item == item.Parent.Left)
                        {
                            item.Parent.StartPosition = item.EndPosition;

                            next = (item.Parent.Node as BinaryExpressionNode).Right;
                        }
                        else
                        {
                            if (item.Parent.Left == null)
                                item.Parent.EndPosition = item.StartPosition;
                            else
                                item.Parent.StartPosition += (item.StartPosition - item.Parent.EndPosition) / 2;
                        }
                    }
                }
                else
                    next = null;
            }

            Console.SetCursorPosition(0, rootTop + 2 * last.Count - 1);
        }

        private static void Print(NodeInfo item, int top)
        {
            SwapColors();
            Print(item.Text, top, item.StartPosition);
            SwapColors();

            if (!(item.Left is null))
                PrintLink(top + 1, "┌", "┘", item.Left.StartPosition + item.Left.Size / 2, item.StartPosition);

            if (!(item.Right is null))
                PrintLink(top + 1, "└", "┐", item.EndPosition - 1, item.Right.EndPosition + item.Right.Size / 2);
        }

        private static void PrintLink(int top, string start, string end, int startPosition, int endPosition)
        {
            Print(start, top, startPosition);
            Print("─", top, startPosition + 1, endPosition);
            Print(end, top, endPosition);
        }
        
        private static void Print(string s, int top, int left, int right = -1)
        {
            Console.SetCursorPosition(left, top);

            if (right < 0)
                right = left + s.Length;

            while (Console.CursorLeft < right)
                Console.Write(s);
        }

        private static void SwapColors()
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = color;
        }
    }    
}