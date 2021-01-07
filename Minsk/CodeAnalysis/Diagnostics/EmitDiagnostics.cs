using System;
using System.Linq;

using Minsk.CodeAnalysis.Emit;
using Minsk.CodeAnalysis.Symbols;

using Mono.Cecil;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public sealed class EmitDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal EmitDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        private void Error(string message)
            => bag.Report(new EmitError(message));

        public void InvalidReference(string reference)
            => Error($"Invalid reference: '{reference}'");

        public void RequiredTypeNotFound(string metadataName)
            => Error($"Required type '{metadataName}' is missing in referenced assemblies");

        public void RequiredTypeNotFound(TypeSymbol type, string metadataName)
            => Error($"Required type '{type.Name}':'{metadataName}' is missing in referenced assemblies");

        public void RequiredTypeAmbiguous(string metadataName, TypeDefinition[] foundTypes)
        {
            var assemblyNameList = string.Join(", ", foundTypes.Select(t => t.Module.Assembly.Name));
            Error($"Required type '{metadataName}' is ambiguous, multiple definitions found; {assemblyNameList}");
        }

        public void RequiredTypeAmbiguous(TypeSymbol type, string metadataName, TypeDefinition[] foundTypes)
        {
            var assemblyNameList = string.Join(", ", foundTypes.Select(t => t.Module.Assembly.Name));
            Error($"Required type '{type.Name}':'{metadataName}' is ambiguous, multiple definitions found; {assemblyNameList}");
        }

        public void MethodNotFound(string name)
            => Error($"Method not found '{name}'");

        public void MethodWithParametersNotFound(string name, string[] parameterTypes)
            => Error($"Method with matching parameters not found, {name}({string.Join(", ", parameterTypes)})");
    }
}