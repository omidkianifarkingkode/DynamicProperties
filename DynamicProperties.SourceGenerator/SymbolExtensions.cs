using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SymbolExtensions
    {
        public static AttributeData GetAttribute(
            this ISymbol symbol,
            INamedTypeSymbol attributeType)
        {
            return symbol
                .GetAttributes()
                .FirstOrDefault(attribute =>
                    SymbolEqualityComparer.Default.Equals(
                        attribute.AttributeClass,
                        attributeType));
        }

        public static bool HasAttribute(
            this ISymbol symbol,
            INamedTypeSymbol attributeType)
        {
            return symbol.GetAttribute(attributeType) != null;
        }
    }
}