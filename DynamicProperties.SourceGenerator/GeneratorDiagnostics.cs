using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class GeneratorDiagnostics
    {
        private static readonly DiagnosticDescriptor MissingPropertyTypeType =
            new DiagnosticDescriptor(
                id: "DP1001",
                title: "PropertyType attribute missing type",
                messageFormat:
                    "Enum member '{0}' has [PropertyType(...)] but no typeof(T) argument.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor UnsupportedPropertyType =
            new DiagnosticDescriptor(
                id: "DP1002",
                title: "Unsupported PropertyType",
                messageFormat:
                    "Enum member '{0}' uses unsupported PropertyType '{1}'.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor EmptyGroupName =
            new DiagnosticDescriptor(
                id: "DP1101",
                title: "Empty property group name",
                messageFormat:
                    "Enum member '{0}' declares [Group] with an empty group name.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GroupOnNonAggregateProperty =
            new DiagnosticDescriptor(
                id: "DP1102",
                title: "Group used on non-aggregate property",
                messageFormat:
                    "Enum member '{0}' belongs to group '{1}', but PropertyType '{2}' is not an aggregate type.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MissingGroupComponent =
            new DiagnosticDescriptor(
                id: "DP1103",
                title: "Missing GroupComponent",
                messageFormat:
                    "Enum member '{0}' belongs to group '{1}' but does not declare [GroupComponent].",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GroupComponentWithoutGroup =
            new DiagnosticDescriptor(
                id: "DP1104",
                title: "GroupComponent used without Group",
                messageFormat:
                    "Enum member '{0}' declares [GroupComponent] but does not belong to a [Group].",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidGroupComponent =
            new DiagnosticDescriptor(
                id: "DP1105",
                title: "Invalid group component",
                messageFormat:
                    "Component '{0}' is not valid for {1} group '{2}'. Expected components: {3}.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor DuplicateGroupComponent =
            new DiagnosticDescriptor(
                id: "DP1106",
                title: "Duplicate group component",
                messageFormat:
                    "Group '{0}' defines component '{1}' more than once.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MissingRequiredGroupComponent =
            new DiagnosticDescriptor(
                id: "DP1107",
                title: "Missing required group component",
                messageFormat:
                    "Group '{0}' of type {1} is missing required component '{2}'.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MixedGroupTypes =
            new DiagnosticDescriptor(
                id: "DP1108",
                title: "Mixed aggregate types in group",
                messageFormat:
                    "Group '{0}' contains mixed aggregate types: {1}.",
                category: "DynamicProperty.SourceGen",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidGroupComponentValue =
            new DiagnosticDescriptor(
                id: "DP1109",
                title: "Invalid GroupComponent value",
                messageFormat:
                    "Enum member '{0}' declares an invalid GroupComponent value.",
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

        public static Diagnostic MissingType(
            IFieldSymbol field)
        {
            return Diagnostic.Create(
                MissingPropertyTypeType,
                field.Locations.Length > 0
                    ? field.Locations[0]
                    : Location.None,
                field.Name);
        }

        public static Diagnostic UnsupportedType(
            IFieldSymbol field,
            ITypeSymbol type)
        {
            return Diagnostic.Create(
                UnsupportedPropertyType,
                field.Locations.Length > 0
                    ? field.Locations[0]
                    : Location.None,
                field.Name,
                type.ToDisplayString());
        }

        public static Diagnostic EmptyGroup(
            PropertyModel property)
                {
                    return Diagnostic.Create(
                        EmptyGroupName,
                        GetLocation(property.Symbol),
                        property.Name);
                }

        public static Diagnostic NonAggregateGroupMember(
            PropertyModel property)
        {
            return Diagnostic.Create(
                GroupOnNonAggregateProperty,
                GetLocation(property.Symbol),
                property.Name,
                property.GroupName,
                property.DeclaredType.ToDisplayString());
        }

        public static Diagnostic MissingComponent(
            PropertyModel property)
        {
            return Diagnostic.Create(
                MissingGroupComponent,
                GetLocation(property.Symbol),
                property.Name,
                property.GroupName);
        }

        public static Diagnostic ComponentWithoutGroup(
            PropertyModel property)
        {
            return Diagnostic.Create(
                GroupComponentWithoutGroup,
                GetLocation(property.Symbol),
                property.Name);
        }

        public static Diagnostic InvalidComponent(
            PropertyModel property,
            AggregateKind kind,
            string expected)
        {
            return Diagnostic.Create(
                InvalidGroupComponent,
                GetLocation(property.Symbol),
                property.GroupComponent,
                kind,
                property.GroupName,
                expected);
        }

        public static Diagnostic DuplicateComponent(
            PropertyModel property,
            string groupName,
            GroupComponentKind component)
        {
            return Diagnostic.Create(
                DuplicateGroupComponent,
                GetLocation(property.Symbol),
                groupName,
                component);
        }

        public static Diagnostic MissingRequiredComponent(
            PropertyModel property,
            string groupName,
            AggregateKind kind,
            GroupComponentKind component)
        {
            return Diagnostic.Create(
                MissingRequiredGroupComponent,
                GetLocation(property.Symbol),
                groupName,
                kind,
                component);
        }

        public static Diagnostic MixedTypes(
            PropertyModel property,
            string groupName,
            string kinds)
        {
            return Diagnostic.Create(
                MixedGroupTypes,
                GetLocation(property.Symbol),
                groupName,
                kinds);
        }

        public static Diagnostic InvalidComponentValue(
            PropertyModel property)
        {
            return Diagnostic.Create(
                InvalidGroupComponentValue,
                GetLocation(property.Symbol),
                property.Name);
        }

        private static Location GetLocation(IFieldSymbol field)
        {
            return field.Locations.Length > 0
                ? field.Locations[0]
                : Location.None;
        }

        public static Diagnostic MethodCollision(
            IFieldSymbol symbol,
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
    }
}