using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;

using BinOp = Minsk.CodeAnalysis.Binding.BoundBinaryOperatorKind;
using UnaOp = Minsk.CodeAnalysis.Binding.BoundUnaryOperatorKind;

namespace Minsk.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram program;
        private readonly Dictionary<VariableSymbol, object> globals;

        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> functions
            = new Dictionary<FunctionSymbol, BoundBlockStatement>();

        private readonly Stack<Dictionary<VariableSymbol, object>> stack
            = new Stack<Dictionary<VariableSymbol, object>>();

        private readonly Random random = new Random();

        private object lastValue = null;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> globalVariables)
        {
            this.program = program ?? throw new ArgumentNullException(nameof(program));

            globals = globalVariables;

            var current = program;
            while (current is not null)
            {
                foreach (var kv in current.Functions)
                {
                    var function = kv.Key;
                    var body = kv.Value;
                    functions.Add(function, body);
                }

                current = current.Previous;
            }
        }

        public object Evaluate()
        {
            var locals = new Dictionary<VariableSymbol, object>();
            stack.Push(locals);
            return EvaluateStatement(program.Statement);
        }

        private object EvaluateStatement(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (var i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }

            var index = 0;

            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];

                switch (s.Kind)
                {
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = s as BoundConditionalGotoStatement;
                        var condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;

                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement(s as BoundExpressionStatement);
                        index++;
                        break;

                    case BoundNodeKind.GotoStatement:
                        var gs = s as BoundGotoStatement;
                        index = labelToIndex[gs.Label];
                        break;

                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;

                    case BoundNodeKind.ReturnStatement:
                        var rs = s as BoundReturnStatement;
                        var value = rs.Expression is not null ? EvaluateExpression(rs.Expression) : null;
                        return value;

                    case BoundNodeKind.VariableDeclarationStatement:
                        EvaluateVariableDeclarationStatement(s as BoundVariableDeclarationStatement);
                        index++;
                        break;

                    default:
                        throw new NotSupportedException(s.Kind.ToString());
                }
            }

            //EvaluateStatement(root);
            return lastValue;
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            lastValue = EvaluateExpression(node.Expression);
        }

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            var value = EvaluateExpression(node.Initializer);
            lastValue = value;
            Assign(node.Variable, value);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch {
                BoundNodeKind.AssignmentExpression
                    => EvaluateAssignmentExpression(node as BoundAssignmentExpression),

                BoundNodeKind.BinaryExpression
                    => EvaluateBinaryExpression(node as BoundBinaryExpression),

                BoundNodeKind.CallExpression
                    => EvaluateCallExpression(node as BoundCallExpression),

                BoundNodeKind.ConversionExpression
                    => EvaluateConversionExpression(node as BoundConversionExpression),

                BoundNodeKind.LiteralExpression
                    => EvaluateLiteralExpression(node as BoundLiteralExpression),

                BoundNodeKind.VariableExpression
                    => EvaluateVariableExpression(node as BoundVariableExpression),

                BoundNodeKind.UnaryExpression
                    => EvaluateUnaryExpression(node as BoundUnaryExpression),

                _   => throw new NotImplementedException(
                        $"{node.Kind} not implemented in EvaluateExpression")
            };
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            Assign(node.Variable, value);
            return value;
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression node)
        {
            var left  = EvaluateExpression(node.Left);
            var right = EvaluateExpression(node.Right);

            var op = node.Op.Kind;

            return op switch {
                BinOp.Addition when node.Left.Type == TypeSymbol.Int && node.Right.Type == TypeSymbol.Int
                    => (int)left  +  (int)right,

                BinOp.Subtraction     => (int)left  -  (int)right,
                BinOp.Multiplication  => (int)left  *  (int)right,
                BinOp.Division        => (int)left  /  (int)right,

                BinOp.LogicalAnd      => (bool)left && (bool)right,
                BinOp.LogicalOr       => (bool)left || (bool)right,
                BinOp.Less            => (int)left  <  (int)right,
                BinOp.LessOrEquals    => (int)left  <= (int)right,
                BinOp.Greater         => (int)left  >  (int)right,
                BinOp.GreaterOrEquals => (int)left  >= (int)right,

                BinOp.Equals          => Equals(left, right),
                BinOp.NotEquals       => !Equals(left, right),

                BinOp.BitwiseAnd when node.Left.Type == TypeSymbol.Int   => (int) left & (int) right,
                BinOp.BitwiseOr  when node.Left.Type == TypeSymbol.Int   => (int) left | (int) right,
                BinOp.BitwiseXor when node.Left.Type == TypeSymbol.Int   => (int) left ^ (int) right,
                BinOp.BitwiseAnd when node.Left.Type == TypeSymbol.Bool  => (bool)left & (bool)right,
                BinOp.BitwiseOr  when node.Left.Type == TypeSymbol.Bool  => (bool)left | (bool)right,
                BinOp.BitwiseXor when node.Left.Type == TypeSymbol.Bool  => (bool)left ^ (bool)right,

                BinOp.Addition when node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String
                    => (string)left + (string)right,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateBinaryExpression")
            };
        }

        private object EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function.IsBuiltin)
                return EvaluateBuiltinFunction(node);

            var locals = new Dictionary<VariableSymbol, object>();
            for (var i = 0; i < node.Arguments.Length; i++)
            {
                var argumentExpression = node.Arguments[i];
                var value = EvaluateExpression(argumentExpression);
                var parameter = node.Function.Parameters[i];
                locals.Add(parameter, value);
            }

            stack.Push(locals);

            var body = functions[node.Function];
            var result = EvaluateStatement(body);

            stack.Pop();

            return result;
        }

        private object EvaluateBuiltinFunction(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(value.ToString());
                return null;
            }
            else if (node.Function == BuiltinFunctions.Rand)
            {
                var value = (int)EvaluateExpression(node.Arguments[0]);
                return random.Next(value);
            }
            else
            {
                throw new Exception($"Unexpected function '{node.Function.Name}'");
            }
        }

        private object EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);

            if (node.Type == TypeSymbol.Any)
                return value;
            else if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);
            else if (node.Type == TypeSymbol.Int)
                return Convert.ToInt32(value);
            else if (node.Type == TypeSymbol.String)
                return Convert.ToString(value);

            throw new Exception($"Unexpected type '{node.Type}'");
        }

        private object EvaluateLiteralExpression(BoundLiteralExpression node)
            => node.Value;

        private object EvaluateVariableExpression(BoundVariableExpression node)
        {
            if (node.Variable.Kind == SymbolKind.GlobalVariable)
                return globals[node.Variable];
            else
            {
                var locals = stack.Peek();
                return locals[node.Variable];
            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression node)
        {
            var value = EvaluateExpression(node.Operand);
            var op = node.Op.Kind;

            return op switch {
                UnaOp.Identity           => (int)value,
                UnaOp.Negation           => -(int)value,

                UnaOp.LogicalNegation    => !(bool)value,

                UnaOp.OnesCompliment     => ~(int)value,

                _ => throw new NotImplementedException(
                    $"'{op}' not implemented in EvaluateUnaryExpression")
            };
        }

        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
                globals[variable] = value;
            else
            {
                var locals = stack.Peek();
                locals[variable] = value;
            }
        }
    }
}