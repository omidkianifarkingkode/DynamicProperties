using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class GeneratorDiagnostics
    {
        private static readonly DiagnosticDescriptor UnsupportedPropertyType =
            new DiagnosticDescriptor(
                id: "DP1002",
                title: "Unsupported property type",
                messageFormat:
                    "Schema property '{0}' uses unsupported property type '{1}'.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GeneratedMethodCollision =
            new DiagnosticDescriptor(
                id: "DP1201",
                title: "Generated method name collision",
                messageFormat:
                    "Generated method '{0}' collides between {1} and {2} in schema '{3}'.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MissingPropertyAttributeDescriptor =
            new DiagnosticDescriptor(
                id: "DP1301",
                title: "Missing Property attribute",
                messageFormat:
                    "Schema property '{0}' must declare [Property(id)].",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidLogicalIdDescriptor =
            new DiagnosticDescriptor(
                id: "DP1302",
                title: "Invalid Property ID",
                messageFormat:
                    "Schema property '{0}' declares invalid logical property ID '{1}'. IDs must be between 1 and {2}.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor DuplicateLogicalIdDescriptor =
            new DiagnosticDescriptor(
                id: "DP1304",
                title: "Duplicate Property ID",
                messageFormat:
                    "Logical property ID '{0}' is used by both schema property '{1}' and schema property '{2}'.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor IndexerPropertyDescriptor =
            new DiagnosticDescriptor(
                id: "DP1305",
                title: "Indexer schema property",
                messageFormat:
                    "Schema property '{0}' is an indexer. DynamicProperty schemas only support normal instance properties.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor StaticPropertyDescriptor =
            new DiagnosticDescriptor(
                id: "DP1306",
                title: "Static schema property",
                messageFormat:
                    "Schema property '{0}' is static. DynamicProperty schemas only support normal instance properties.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor SetterPropertyDescriptor =
            new DiagnosticDescriptor(
                id: "DP1307",
                title: "Schema property has setter",
                messageFormat:
                    "Schema property '{0}' declares a setter. DynamicProperty schema properties must be getter-only.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor SchemaInheritanceDescriptor =
            new DiagnosticDescriptor(
                id: "DP1308",
                title: "Schema inheritance is not supported",
                messageFormat:
                    "Schema interface '{0}' inherits '{1}'. DynamicProperty schema composition is not supported in this version.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static Diagnostic UnsupportedType(
            ISymbol symbol,
            ITypeSymbol type)
        {
            return Diagnostic.Create(
                UnsupportedPropertyType,
                GetLocation(symbol),
                symbol.Name,
                type.ToDisplayString());
        }

        public static Diagnostic MethodCollision(
            ISymbol symbol,
            string methodName,
            string firstOrigin,
            string secondOrigin,
            string schemaName)
        {
            return Diagnostic.Create(
                GeneratedMethodCollision,
                GetLocation(symbol),
                methodName,
                firstOrigin,
                secondOrigin,
                schemaName);
        }

        public static Diagnostic MissingPropertyAttribute(
            IPropertySymbol property)
        {
            return Diagnostic.Create(
                MissingPropertyAttributeDescriptor,
                GetLocation(property),
                property.Name);
        }

        public static Diagnostic InvalidLogicalId(
            PropertyModel property,
            int maxLogicalId)
        {
            return Diagnostic.Create(
                InvalidLogicalIdDescriptor,
                GetLocation(property.Symbol),
                property.Name,
                property.LogicalId,
                maxLogicalId);
        }

        public static Diagnostic DuplicateLogicalId(
            PropertyModel first,
            PropertyModel second)
        {
            return Diagnostic.Create(
                DuplicateLogicalIdDescriptor,
                GetLocation(second.Symbol),
                second.LogicalId,
                first.Name,
                second.Name);
        }

        public static Diagnostic IndexerProperty(
            IPropertySymbol property)
        {
            return Diagnostic.Create(
                IndexerPropertyDescriptor,
                GetLocation(property),
                property.Name);
        }

        public static Diagnostic StaticProperty(
            IPropertySymbol property)
        {
            return Diagnostic.Create(
                StaticPropertyDescriptor,
                GetLocation(property),
                property.Name);
        }

        public static Diagnostic SetterProperty(
            IPropertySymbol property)
        {
            return Diagnostic.Create(
                SetterPropertyDescriptor,
                GetLocation(property),
                property.Name);
        }

        public static Diagnostic SchemaInheritance(
            INamedTypeSymbol schema,
            INamedTypeSymbol inherited)
        {
            return Diagnostic.Create(
                SchemaInheritanceDescriptor,
                GetLocation(schema),
                schema.ToDisplayString(),
                inherited.ToDisplayString());
        }

        private static Location GetLocation(ISymbol symbol)
        {
            return symbol.Locations.Length > 0
                ? symbol.Locations[0]
                : Location.None;
        }
    }
}
