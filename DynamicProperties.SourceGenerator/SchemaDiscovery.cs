using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SchemaDiscovery
    {
        public static IReadOnlyList<INamedTypeSymbol> Discover(
            Compilation compilation,
            SchemaEnumSyntaxReceiver receiver,
            GeneratorSymbols symbols)
        {
            var result =
                new List<INamedTypeSymbol>();

            var seen =
                new HashSet<INamedTypeSymbol>(
                    SymbolEqualityComparer.Default);

            foreach (var syntax in receiver.Candidates)
            {
                var semanticModel =
                    compilation.GetSemanticModel(
                        syntax.SyntaxTree);

                var enumSymbol =
                    semanticModel.GetDeclaredSymbol(syntax)
                        as INamedTypeSymbol;

                if (enumSymbol == null)
                    continue;

                if (enumSymbol.TypeKind != TypeKind.Enum)
                    continue;

                if (!IsDynamicPropertySchema(
                        enumSymbol,
                        symbols))
                {
                    continue;
                }

                if (seen.Add(enumSymbol))
                {
                    result.Add(enumSymbol);
                }
            }

            return result;
        }

        private static bool IsDynamicPropertySchema(
            INamedTypeSymbol enumSymbol,
            GeneratorSymbols symbols)
        {
            return enumSymbol
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Any(field =>
                    field.ConstantValue != null &&
                    field.HasAttribute(
                        symbols.PropertyTypeAttribute));
        }
    }
}