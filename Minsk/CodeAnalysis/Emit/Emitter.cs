using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Diagnostics;
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

        private readonly List<AssemblyDefinition> assemblyReferences = new List<AssemblyDefinition>();

        private readonly AssemblyDefinition assemblyDefinition;
        private readonly Dictionary<TypeSymbol, (TypeDefinition Definition, TypeReference Reference)> knownTypes;
        private readonly (TypeDefinition Definition, TypeReference Reference) consoleType;
        private readonly (MethodDefinition Definition, MethodReference Reference) consoleType_WriteLine;
        private readonly List<(TypeSymbol Type, string MetadataName)> builtinTypes
            = new List<(TypeSymbol Type, string MetadataName)> {
                (TypeSymbol.Any,    "System.Object"),
                (TypeSymbol.Bool,   "System.Boolean"),
                (TypeSymbol.Int,    "System.Int32"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void,   "System.Void"),
            };

        private Dictionary<FunctionSymbol, MethodDefinition> methods;

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

            knownTypes = builtinTypes.ToDictionary(
                item => item.Type,
                item => ResolveType(item.Type, item.MetadataName));

            consoleType = ResolveType("System.Console");
            consoleType_WriteLine = ResolveMethod(consoleType.Definition, "WriteLine", new[] { "System.String" });
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

            var objectType = knownTypes[TypeSymbol.Any].Reference;
            var typeDefinition = new TypeDefinition(
                "",
                "Program",
                TypeAttributes.Abstract | TypeAttributes.Sealed,
                objectType);

            assemblyDefinition.MainModule.Types.Add(typeDefinition);

            methods = new Dictionary<FunctionSymbol, MethodDefinition>(program.Functions.Count);

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
            var voidType = knownTypes[TypeSymbol.Void].Reference;
            var main = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, voidType);
            typeDefinition.Methods.Add(main);
            methods.Add(function, main);
        }

        private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
        {
            if (!methods.TryGetValue(function, out var method))
                throw new Exception();

            var ilProcessor = method.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcessor, statement);

            if (function.ReturnType.IsVoidType)
                ilProcessor.Emit(OpCodes.Ret);

            method.Body.OptimizeMacros();
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
                break;
            }
        }

        private void EmitConditionalGotoStatement(
            ILProcessor ilProcessor, BoundConditionalGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Expression.Type.IsNotVoidType)
                ilProcessor.Emit(OpCodes.Pop);
        }

        private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitVariableDeclarationStatement(
            ILProcessor ilProcessor, BoundVariableDeclarationStatement node)
        {
            throw new NotImplementedException();
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
                break;
            }
        }

        private void EmitAssignmentExpression(
            ILProcessor ilProcessor, BoundAssignmentExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
        {
            foreach (var argument in node.Arguments)
            {
                EmitExpression(ilProcessor, argument);
            }

            if (node.Function == BuiltinFunctions.Print)
            {
                ilProcessor.Emit(OpCodes.Call, consoleType_WriteLine.Definition);
            }
            else if (node.Function == BuiltinFunctions.Input)
            {
                //ilProcessor.Emit(OpCodes.Call)
                throw new NotImplementedException();
            }
            else if (node.Function == BuiltinFunctions.Rand)
            {
                throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
        {
            throw new NotImplementedException();
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
