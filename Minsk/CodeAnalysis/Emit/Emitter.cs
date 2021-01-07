using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Symbols;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Minsk.CodeAnalysis.Emit
{
    internal class Emitter
    {
        private readonly DiagnosticBag diagnostics;

        private readonly List<AssemblyDefinition> assemblyReferences = new List<AssemblyDefinition>();

        private AssemblyDefinition assemblyDefinition;

        private Emitter(DiagnosticBag diagnostics)
        {
            this.diagnostics = diagnostics;
        }

        private EmitDiagnostics Report => diagnostics.Emit;


        public static EmitResult Emit(
            BoundProgram boundProgram,
            string moduleName,
            IEnumerable<string> references,
            string outputPath)
        {
            var diagnostics = boundProgram.Diagnostics;
            var emitter = new Emitter(diagnostics);
            return emitter.EmitAssembly(moduleName, references, outputPath);
        }

        public EmitResult EmitAssembly(
            string moduleName,
            IEnumerable<string> references,
            string outputPath)
        {
            var assemblyNameDefinition = new AssemblyNameDefinition(moduleName, new Version(1, 0));

            assemblyDefinition = AssemblyDefinition.CreateAssembly(
                assemblyNameDefinition, moduleName, ModuleKind.Console);

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

            var builtinTypes = new List<(TypeSymbol Type, string MetadataName)> {
                (TypeSymbol.Any,    "System.Object"),
                (TypeSymbol.Bool,   "System.Boolean"),
                (TypeSymbol.Int,    "System.Int32"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void,   "System.Void"),
            };

            var knownTypes = new Dictionary<TypeSymbol, TypeReference>();

            foreach (var (type, metadataName) in builtinTypes)
            {
                var resolvedType = ResolveType(type, metadataName);
                knownTypes.Add(type, resolvedType.Reference);
            }

            var consoleType = ResolveType("System.Console");
            var consoleType_WriteLine = ResolveMethod(consoleType.Definition, "WriteLine", new[] { "System.String" });

            if (diagnostics.Any())
                return new EmitResult(diagnostics);

            var typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, knownTypes[TypeSymbol.Any]);
            assemblyDefinition.MainModule.Types.Add(typeDefinition);

            var voidType = knownTypes[TypeSymbol.Void];
            var main = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);
            typeDefinition.Methods.Add(main);

            var ilProcessor = main.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Ldstr, "Hello, Worlds");
            ilProcessor.Emit(OpCodes.Call, consoleType_WriteLine.Reference);
            ilProcessor.Emit(OpCodes.Ret);

            assemblyDefinition.EntryPoint = main;

            assemblyDefinition.Write(outputPath);

            return new EmitResult(diagnostics);
        }

        private (MethodDefinition Definition, MethodReference Reference) ResolveMethod(
            TypeDefinition typeDefinition,
            string name,
            string[] parameterTypeNames)
        {
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
                Report.RequiredTypeNotFound(metadataName);
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
