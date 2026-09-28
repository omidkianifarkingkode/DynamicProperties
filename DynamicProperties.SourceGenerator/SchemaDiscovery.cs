using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SchemaDiscovery
    {
        public static IReadOnlyList<INamedTypeSymbol> Discover(
            Compilation compilation,
            SchemaSyntaxReceiver receiver,
            GeneratorSymbols symbols)
        {
            var result =
                new List<INamedTypeSymbol>();

            var seen =
                new HashSet<INamedTypeSymbol>(
                    SymbolEqualityComparer.Default);

            foreach (var syntax in receiver.InterfaceCandidates)
            {
                var semanticModel =
                    compilation.GetSemanticModel(
                        syntax.SyntaxTree);

                var interfaceSymbol =
                    semanticModel.GetDeclaredSymbol(syntax)
                        as INamedTypeSymbol;

                if (interfaceSymbol == null)
                    continue;

                if (interfaceSymbol.TypeKind != TypeKind.Interface)
                    continue;

                if (!IsTypedPropertySchema(
                        interfaceSymbol,
                        symbols))
                {
                    continue;
                }

                if (seen.Add(interfaceSymbol))
                    result.Add(interfaceSymbol);
            }

            return result;
        }

        private static bool IsTypedPropertySchema(
            INamedTypeSymbol interfaceSymbol,
            GeneratorSymbols symbols)
        {
            return interfaceSymbol
                .AllInterfaces
                .Contains(
                    symbols.PropertySchema,
                    SymbolEqualityComparer.Default);
        }
    }
}
