using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Minsk.CodeAnalysis.Emit
{
    internal sealed class Emitter
    {
        private readonly string moduleName;
        private readonly string[] references;
        private readonly string outputPath;
        private readonly DiagnosticBag diagnostics;

        private readonly MethodReference objectEquals;
        private readonly MethodReference consoleReadLine;
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference stringConcat2;
        private readonly MethodReference stringConcat3;
        private readonly MethodReference stringConcat4;
        private readonly MethodReference stringConcatArray;
        private readonly MethodReference convertToBoolean;
        private readonly MethodReference convertToInt32;
        private readonly MethodReference convertToString;
        private readonly TypeReference random;
        private readonly MethodReference randomConstructor;
        private readonly MethodReference randomNext;
        private readonly AssemblyDefinition assemblyDefinition;

        private readonly Dictionary<TypeSymbol, (TypeDefinition Definition, TypeReference Reference)> knownTypes
            = new Dictionary<TypeSymbol, (TypeDefinition Definition, TypeReference Reference)>();

        private readonly Dictionary<FunctionSymbol, MethodDefinition> methods
            = new Dictionary<FunctionSymbol, MethodDefinition>();

        private readonly Dictionary<VariableSymbol, VariableDefinition> locals
            = new Dictionary<VariableSymbol, VariableDefinition>();

        private readonly Dictionary<BoundLabel, int> labels = new Dictionary<BoundLabel, int>();

        private readonly List<(int InstructionIndex, BoundLabel Target)> fixups
            = new List<(int InstructionIndex, BoundLabel Target)>();

        private readonly List<AssemblyDefinition> assemblyReferences = new List<AssemblyDefinition>();

        private readonly List<(TypeSymbol Type, string MetadataName)> builtinTypes
            = new List<(TypeSymbol Type, string MetadataName)> {
                (TypeSymbol.Any,    "System.Object"),
                (TypeSymbol.Bool,   "System.Boolean"),
                (TypeSymbol.Int,    "System.Int32"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void,   "System.Void"),
            };

        private TypeDefinition typeDefinition;
        //private FieldDefinition randomFieldDefinition;


        private Emitter(
            string moduleName,
            IEnumerable<string> references,
            string outputPath,
            DiagnosticBag diagnostics)
        {
            // Save parameters
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException(nameof(moduleName));
            this.moduleName = moduleName;

            this.references = references?.ToArray()
                ?? throw new ArgumentNullException(nameof(references));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException(nameof(outputPath));
            this.outputPath = outputPath;

            this.diagnostics = diagnostics;

            // Read referenced assemblies
            foreach (var reference in references)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    assemblyReferences.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    Report.InvalidReference(reference);
                }
            }

            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));

            assemblyDefinition = AssemblyDefinition.CreateAssembly(
                assemblyName, moduleName, ModuleKind.Console);

            foreach (var (typeSymbol, metadataName) in builtinTypes)
            {
                var (definition, reference) = ResolveType(typeSymbol, metadataName);
                knownTypes.Add(typeSymbol, (definition, reference));
            }

            objectEquals = ResolveMethod("System.Object", "Equals", "System.Object", "System.Object");
            consoleReadLine = ResolveMethod("System.Console", "ReadLine");
            consoleWriteLine = ResolveMethod("System.Console", "WriteLine", "System.Object");
            stringConcat2 = ResolveMethod("System.String", "Concat", "System.String", "System.String");
            stringConcat3 = ResolveMethod("System.String", "Concat", "System.String", "System.String", "System.String");
            stringConcat4 = ResolveMethod("System.String", "Concat", "System.String", "System.String", "System.String", "System.String");
            stringConcatArray = ResolveMethod("System.String", "Concat", "System.String[]");
            convertToBoolean = ResolveMethod("System.Convert", "ToBoolean", "System.Object");
            convertToInt32 = ResolveMethod("System.Convert", "ToInt32", "System.Object");
            convertToString = ResolveMethod("System.Convert", "ToString", "System.Object");
            (_, random) = ResolveType(TypeSymbol.Random, "System.Random");
            randomConstructor = ResolveMethod("System.Random", ".ctor");
            randomNext = ResolveMethod("System.Random", "Next", "System.Int32");

            var objectType = knownTypes[TypeSymbol.Any].Reference;

            if (objectType != null)
            {
                typeDefinition = new TypeDefinition(
                    "",
                    "Program",
                    TypeAttributes.Abstract | TypeAttributes.Sealed,
                    objectType);
                assemblyDefinition.MainModule.Types.Add(typeDefinition);
            }
        }

        private EmitDiagnostics Report => diagnostics.Emit;


        public static EmitResult Emit(
            BoundProgram boundProgram,
            string moduleName,
            IEnumerable<string> references,
            string outputPath)
        {
            var emitter = new Emitter(
                moduleName,
                references,
                outputPath,
                boundProgram.Diagnostics);

            return emitter.EmitAssembly(boundProgram);
        }

        public EmitResult EmitAssembly(BoundProgram program)
        {
            if (diagnostics.Any())
                return new EmitResult(diagnostics);

            foreach (var (function, body) in program.FunctionDefinitions)
                EmitFunction(typeDefinition, function);

            foreach (var (function, body) in program.FunctionDefinitions)
                EmitFunctionBody(function, body);

            if (methods.TryGetValue(program.MainFunction, out var mainMethod))
                assemblyDefinition.EntryPoint = mainMethod;

            assemblyDefinition.Write(outputPath);

            return new EmitResult(diagnostics);
        }

        private void EmitFunction(TypeDefinition typeDefinition, FunctionSymbol function)
        {
            var functionType = knownTypes[function.ReturnType].Definition;
            var method = new MethodDefinition(
                function.Name,
                MethodAttributes.Static | MethodAttributes.Private,
                functionType);

            foreach (var parameter in function.Parameters)
            {
                var parameterType = knownTypes[parameter.Type].Definition;
                var parameterAttributes = ParameterAttributes.None;
                var parameterDefinition =
                    new ParameterDefinition(parameter.Name, parameterAttributes, parameterType);
            }

            typeDefinition.Methods.Add(method);
            methods.Add(function, method);
        }

        private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
        {
            if (!methods.TryGetValue(function, out var method))
                throw new Exception();

            locals.Clear();
            labels.Clear();
            fixups.Clear();

            var ilProcessor = method.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcessor, statement);

            foreach (var fixup in fixups)
            {
                var targetLabel = fixup.Target;
                var targetInstructionIndex = labels[targetLabel];
                var targetInstruction = ilProcessor.Body.Instructions[targetInstructionIndex];
                var instructionToFixup = ilProcessor.Body.Instructions[fixup.InstructionIndex];
                instructionToFixup.Operand = targetInstruction;
            }

            method.Body.Optimize();
        }

        private void EmitStatement(ILProcessor ilProcessor, BoundStatement node)
        {
            switch (node.Kind)
            {
            case BoundNodeKind.ConditionalGotoStatement:
                EmitConditionalGotoStatement(ilProcessor, (BoundConditionalGotoStatement)node);
                break;

            case BoundNodeKind.ExpressionStatement:
                EmitExpressionStatement(ilProcessor, (BoundExpressionStatement)node);
                break;

            case BoundNodeKind.GotoStatement:
                EmitGotoStatement(ilProcessor, (BoundGotoStatement)node);
                break;

            case BoundNodeKind.LabelStatement:
                EmitLabelStatement(ilProcessor, (BoundLabelStatement)node);
                break;

            case BoundNodeKind.ReturnStatement:
                EmitReturnStatement(ilProcessor, (BoundReturnStatement)node);
                break;

            case BoundNodeKind.VariableDeclarationStatement:
                EmitVariableDeclarationStatement(ilProcessor, (BoundVariableDeclarationStatement)node);
                break;

            default:
                throw new NotImplementedException(
                    $"EmitStatement not implemented for {node.Kind.ToString()}");
            }
        }

        private void EmitConditionalGotoStatement(
            ILProcessor ilProcessor, BoundConditionalGotoStatement node)
        {
            EmitExpression(ilProcessor, node.Condition);

            var opCode = node.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse;
            fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(opCode, Instruction.Create(OpCodes.Nop));
        }

        private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Expression.Type.IsNotVoidType)
                ilProcessor.Emit(OpCodes.Pop);
        }

        private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
        {
            fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }

        private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node)
        {
            labels.Add(node.Label, ilProcessor.Body.Instructions.Count);
        }

        private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
        {
            if (node.Expression != null)
                EmitExpression(ilProcessor, node.Expression);

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitVariableDeclarationStatement(
            ILProcessor ilProcessor, BoundVariableDeclarationStatement node)
        {
            var typeReference = knownTypes[node.Variable.Type].Reference;
            var variableDefinition = new VariableDefinition(typeReference);
            locals.Add(node.Variable, variableDefinition);
            ilProcessor.Body.Variables.Add(variableDefinition);

            EmitExpression(ilProcessor, node.Initializer);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            switch (node.Kind)
            {
            case BoundNodeKind.AssignmentExpression:
                EmitAssignmentExpression(ilProcessor, (BoundAssignmentExpression)node);
                break;

            case BoundNodeKind.BinaryExpression:
                EmitBinaryExpression(ilProcessor, (BoundBinaryExpression)node);
                break;

            case BoundNodeKind.CallExpression:
                EmitCallExpression(ilProcessor, (BoundCallExpression)node);
                break;

            case BoundNodeKind.ConversionExpression:
                EmitConversionExpression(ilProcessor, (BoundConversionExpression)node);
                break;

            case BoundNodeKind.LiteralExpression:
                EmitLiteralExpression(ilProcessor, (BoundLiteralExpression)node);
                break;

            case BoundNodeKind.UnaryExpression:
                EmitUnaryExpression(ilProcessor, (BoundUnaryExpression)node);
                break;

            case BoundNodeKind.VariableExpression:
                EmitVariableExpression(ilProcessor, (BoundVariableExpression)node);
                break;

            default:
                throw new NotImplementedException(
                    $"EmitExpression not implemented for {node.Kind.ToString()}");
            }
        }

        private void EmitAssignmentExpression(
            ILProcessor ilProcessor, BoundAssignmentExpression node)
        {
            var variableDefinition = locals[node.Variable];
            EmitExpression(ilProcessor, node.Expression);
            ilProcessor.Emit(OpCodes.Dup);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            // +(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.Addition)
            {
                if (node.OperandTypesAre(TypeSymbol.String))
                {
                    //EmitStringConcatExpression(ilProcessor.node);
                    //return;
                    throw new NotImplementedException();
                }
            }

            EmitExpression(ilProcessor, node.Left);
            EmitExpression(ilProcessor, node.Right);

            // ==(any, any)
            // ==(stirng, string)
            if (node.Op.Kind == BoundBinaryOperatorKind.Equals)
            {
                if (node.OperandTypesAre(TypeSymbol.Any)
                 || node.OperandTypesAre(TypeSymbol.String))
                {
                    ilProcessor.Emit(OpCodes.Call, objectEquals);
                    return;
                }
            }

            // !=(any, any)
            // !=(string, string)
            if (node.Op.Kind == BoundBinaryOperatorKind.NotEquals)
            {
                if (node.OperandTypesAre(TypeSymbol.Any)
                 || node.OperandTypesAre(TypeSymbol.String))
                {
                    ilProcessor.Emit(OpCodes.Call, objectEquals);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    return;
                }
            }

            switch (node.Op.Kind)
            {
            case BoundBinaryOperatorKind.Addition:
                ilProcessor.Emit(OpCodes.Add);
                break;

            case BoundBinaryOperatorKind.Subtraction:
                ilProcessor.Emit(OpCodes.Sub);
                break;
            case BoundBinaryOperatorKind.Multiplication:
                ilProcessor.Emit(OpCodes.Mul);
                break;

            case BoundBinaryOperatorKind.Division:
                ilProcessor.Emit(OpCodes.Div);
                break;

            case BoundBinaryOperatorKind.Less:
                ilProcessor.Emit(OpCodes.Clt);
                break;

            case BoundBinaryOperatorKind.LessOrEquals:
                ilProcessor.Emit(OpCodes.Cgt);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case BoundBinaryOperatorKind.Greater:
                ilProcessor.Emit(OpCodes.Cgt);
                break;

            case BoundBinaryOperatorKind.GreaterOrEquals:
                ilProcessor.Emit(OpCodes.Clt);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case BoundBinaryOperatorKind.BitwiseAnd:
            case BoundBinaryOperatorKind.LogicalAnd:
                ilProcessor.Emit(OpCodes.And);
                break;

            case BoundBinaryOperatorKind.BitwiseOr:
            case BoundBinaryOperatorKind.LogicalOr:
                ilProcessor.Emit(OpCodes.Or);
                break;

            case BoundBinaryOperatorKind.Equals:
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case BoundBinaryOperatorKind.NotEquals:
                ilProcessor.Emit(OpCodes.Ceq);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case BoundBinaryOperatorKind.BitwiseXor:
                ilProcessor.Emit(OpCodes.Xor);
                break;

            default:
                throw new NotImplementedException(node.Op.Kind.ToString());
            }
        }

        // private void EmitStringConcatExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        // {
        //     // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
        //     // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
        //     // Example: folding b and c in ((a + b) + c) if they are constant.

        //     var nodes = FoldConstants(node., Flatten(node)).ToList();

        //     switch (nodes.Count)
        //     {
        //         case 0:
        //             ilProcessor.Emit(OpCodes.Ldstr, string.Empty);
        //             break;

        //         case 1:
        //             EmitExpression(ilProcessor, nodes[0]);
        //             break;

        //         case 2:
        //             EmitExpression(ilProcessor, nodes[0]);
        //             EmitExpression(ilProcessor, nodes[1]);
        //             ilProcessor.Emit(OpCodes.Call, stringConcat2);
        //             break;

        //         case 3:
        //             EmitExpression(ilProcessor, nodes[0]);
        //             EmitExpression(ilProcessor, nodes[1]);
        //             EmitExpression(ilProcessor, nodes[2]);
        //             ilProcessor.Emit(OpCodes.Call, stringConcat3);
        //             break;

        //         case 4:
        //             EmitExpression(ilProcessor, nodes[0]);
        //             EmitExpression(ilProcessor, nodes[1]);
        //             EmitExpression(ilProcessor, nodes[2]);
        //             EmitExpression(ilProcessor, nodes[3]);
        //             ilProcessor.Emit(OpCodes.Call, stringConcat4);
        //             break;

        //         default:
        //             ilProcessor.Emit(OpCodes.Ldc_I4, nodes.Count);
        //             ilProcessor.Emit(OpCodes.Newarr, knownTypes[TypeSymbol.String].Definition);

        //             for (var i = 0; i < nodes.Count; i++)
        //             {
        //                 ilProcessor.Emit(OpCodes.Dup);
        //                 ilProcessor.Emit(OpCodes.Ldc_I4, i);
        //                 EmitExpression(ilProcessor, nodes[i]);
        //                 ilProcessor.Emit(OpCodes.Stelem_Ref);
        //             }

        //             ilProcessor.Emit(OpCodes.Call, stringConcatArray);
        //             break;
        //     }

        //     // (a + b) + (c + d) --> [a, b, c, d]
        //     static IEnumerable<BoundExpression> Flatten(BoundExpression node)
        //     {
        //         if (node is BoundBinaryExpression binaryExpression &&
        //             binaryExpression.Op.Kind == BoundBinaryOperatorKind.Addition &&
        //             binaryExpression.Left.Type == TypeSymbol.String &&
        //             binaryExpression.Right.Type == TypeSymbol.String)
        //         {
        //             foreach (var result in Flatten(binaryExpression.Left))
        //                 yield return result;

        //             foreach (var result in Flatten(binaryExpression.Right))
        //                 yield return result;
        //         }
        //         else
        //         {
        //             if (node.Type != TypeSymbol.String)
        //                 throw new Exception($"Unexpected node type in string concatenation: {node.Type}");

        //             yield return node;
        //         }
        //     }

        //     // [a, "foo", "bar", b, ""] --> [a, "foobar", b]
        //     static IEnumerable<BoundExpression> FoldConstants(SyntaxNode syntax, IEnumerable<BoundExpression> nodes)
        //     {
        //         StringBuilder sb = null;

        //         foreach (var node in nodes)
        //         {
        //             if (node. != null)
        //             {
        //                 var stringValue = (string)node.ConstantValue.Value;

        //                 if (string.IsNullOrEmpty(stringValue))
        //                     continue;

        //                 sb ??= new StringBuilder();
        //                 sb.Append(stringValue);
        //             }
        //             else
        //             {
        //                 if (sb?.Length > 0)
        //                 {
        //                     yield return new BoundLiteralExpression(sb.ToString());
        //                     sb.Clear();
        //                 }

        //                 yield return node;
        //             }
        //         }

        //         if (sb?.Length > 0)
        //             yield return new BoundLiteralExpression(sb.ToString());
        //     }
        // }

        private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Rand)
            {
                // if (randomField = null)
                //     EmitRandomField();

                // ilProcessor.Emit(OpCodes.Ldsfld, randomField);

                // foreach (var argument in node.Arguments)
                //     EmitExpression(ilProcessor, argument);

                // ilProcessor.Emit(OpCodes.Callvirt, randomNext);
                // return;
                throw new NotImplementedException();
            }

            foreach (var argument in node.Arguments)
                EmitExpression(ilProcessor, argument);

            if (node.Function == BuiltinFunctions.Print)
            {
                ilProcessor.Emit(OpCodes.Call, consoleWriteLine);
            }
            else if (node.Function == BuiltinFunctions.Input)
            {
                ilProcessor.Emit(OpCodes.Call, consoleReadLine);
            }
            else
            {
                if (!methods.TryGetValue(node.Function, out var method))
                    throw new KeyNotFoundException(node.Function.Name);
                ilProcessor.Emit(OpCodes.Call, method);
            }
        }

        private void EmitConversionExpression(
            ILProcessor ilProcessor, BoundConversionExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitLiteralExpression(ILProcessor ilProcessor, BoundLiteralExpression node)
        {
            Debug.Assert(node.Value != null);

            if (node.Type == TypeSymbol.Bool)
            {
                var value = (bool)node.Value;
                var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcessor.Emit(instruction);
            }
            else if (node.Type == TypeSymbol.Int)
            {
                var value = (int)node.Value;
                ilProcessor.Emit(OpCodes.Ldc_I4, value);
            }
            else if (node.Type == TypeSymbol.String)
            {
                var value = (string)node.Value;
                ilProcessor.Emit(OpCodes.Ldstr, value);
            }
            else
            {
                throw new NotImplementedException($"Unexpected type literal: {node.Type.Name}");
            }
        }

        private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
        {
            EmitExpression(ilProcessor, node.Operand);

            switch (node.Op.Kind)
            {
            case BoundUnaryOperatorKind.Identity:
                break;

            case BoundUnaryOperatorKind.LogicalNegation:
            {
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
            } break;

            case BoundUnaryOperatorKind.Negation:
            {
                ilProcessor.Emit(OpCodes.Neg);
            } break;

            case BoundUnaryOperatorKind.OnesCompliment:
            {
                ilProcessor.Emit(OpCodes.Not);
            } break;

            default:
                throw new NotImplementedException(node.Op.Kind.ToString());
            }
        }

        private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                //ilProcessor.Emit(OpCodes.Ldarg, parameter.Ordinal);
                throw new NotImplementedException();
            }
            else
            {
                var variableDefinition = locals[node.Variable];
                ilProcessor.Emit(OpCodes.Ldloc, variableDefinition);
            }
        }

        private MethodReference ResolveMethod(string type, string methodName, params string[] parameterTypes)
        {
            var foundTypes = assemblyReferences
                .SelectMany(a => a.Modules)
                .SelectMany(m => m.Types)
                .Where(t => t.FullName == type)
                .ToArray();

            if (foundTypes.Length == 1)
            {
                var foundType = foundTypes[0];
                var methods = foundType.Methods.Where(m => m.Name == methodName);

                foreach (var method in methods)
                {
                    if (method.Parameters.Count != parameterTypes.Length)
                        continue;

                    var allParametersMatch = true;

                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        if (method.Parameters[i].ParameterType.FullName != parameterTypes[i])
                        {
                            allParametersMatch = false;
                            break;
                        }
                    }

                    if (!allParametersMatch)
                        continue;

                    return assemblyDefinition.MainModule.ImportReference(method);
                }

                Report.MethodWithParametersNotFound(methodName, parameterTypes);
                return null;
            }
            else if (foundTypes.Length == 0)
            {
                Report.MethodWithParametersNotFound(methodName, parameterTypes);
            }
            else
            {
                Report.MethodWithParametersNotFound(methodName, parameterTypes);
            }

            return null;
        }

        private (MethodDefinition Definition, MethodReference Reference) ResolveMethod(
            TypeDefinition typeDefinition,
            string name,
            string[] parameterTypeNames)
        {
            if (typeDefinition is null)
            {
                Report.MethodNotFound(name);
                return (null, null);
            }

            var methods = typeDefinition.Methods.Where(m => m.Name == name);

            if (!methods.Any())
            {
                Report.MethodNotFound(name);
                return (null, null);
            }

            methods = methods.Where(m => m.Parameters.Count == parameterTypeNames.Length);

            bool MatchParameters(MethodDefinition method)
            {
                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var parameter = method.Parameters[i];
                    var parameterTypeName = parameter.ParameterType.FullName;

                    if (parameterTypeName == parameterTypeNames[i])
                        return true;
                }

                return false;
            }

            var methodDefinition = methods.SingleOrDefault(MatchParameters);

            if (methodDefinition is null)
            {
                Report.MethodWithParametersNotFound(name, parameterTypeNames);
                return (null, null);
            }

            var methodReference = assemblyDefinition.MainModule.ImportReference(methodDefinition);
            return (methodDefinition, methodReference);
        }


        private (TypeDefinition Definition, TypeReference Reference) ResolveType(string metadataName)
        {
            var foundTypes =
                   assemblyReferences
                       .SelectMany(a => a.Modules)
                       .SelectMany(m => m.Types)
                       .Where(t => t.FullName == metadataName)
                       .ToArray();

            if (foundTypes.Length == 0)
            {
                Report.RequiredTypeNotFound(metadataName);
                return (null, null);
            }
            else if (foundTypes.Length > 1)
                Report.RequiredTypeAmbiguous(metadataName, foundTypes);

            var typeDefinition = foundTypes[0];
            var typeReference = assemblyDefinition.MainModule.ImportReference(typeDefinition);
            return (typeDefinition, typeReference);
        }


        private (TypeDefinition Definition, TypeReference Reference) ResolveType(TypeSymbol type, string metadataName)
        {
            var foundTypes =
                   assemblyReferences
                       .SelectMany(a => a.Modules)
                       .SelectMany(m => m.Types)
                       .Where(t => t.FullName == metadataName)
                       .ToArray();

            if (foundTypes.Length == 0)
                Report.RequiredTypeNotFound(type, metadataName);
            else if (foundTypes.Length > 1)
                Report.RequiredTypeAmbiguous(type, metadataName, foundTypes);

            var typeDefinition = foundTypes[0];
            var typeReference = assemblyDefinition.MainModule.ImportReference(typeDefinition);
            return (typeDefinition, typeReference);
        }
    }
}
