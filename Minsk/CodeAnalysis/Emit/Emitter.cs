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
    internal sealed class Emitter
    {
        private readonly string moduleName;
        private readonly string[] references;
        private readonly string outputPath;
        private readonly DiagnosticBag diagnostics;

        private readonly List<AssemblyDefinition> assemblyReferences = new List<AssemblyDefinition>();

        private readonly AssemblyDefinition assemblyDefinition;
        private readonly Dictionary<TypeSymbol, (TypeDefinition Definition, TypeReference Reference)> knownTypes;

        private readonly List<(TypeSymbol Type, string MetadataName)> builtinTypes
            = new List<(TypeSymbol Type, string MetadataName)> {
                (TypeSymbol.Any,    "System.Object"),
                (TypeSymbol.Bool,   "System.Boolean"),
                (TypeSymbol.Int,    "System.Int32"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void,   "System.Void"),
            };

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

        public EmitResult EmitAssembly(BoundProgram boundProgram)
        {
            var consoleType = ResolveType("System.Console");
            var consoleType_WriteLine = ResolveMethod(consoleType.Definition, "WriteLine", new[] { "System.String" });

            if (diagnostics.Any())
                return new EmitResult(diagnostics);

            var objectType = knownTypes[TypeSymbol.Any].Reference;
            var typeDefinition = new TypeDefinition(
                "",
                "Program",
                TypeAttributes.Abstract | TypeAttributes.Sealed,
                objectType);

            assemblyDefinition.MainModule.Types.Add(typeDefinition);

            var voidType = knownTypes[TypeSymbol.Void].Reference;
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
