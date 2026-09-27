using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SchemaParser
    {
        public static SchemaModel Parse(
            INamedTypeSymbol schemaSymbol,
            GeneratorSymbols symbols,
            Action<Diagnostic> reportDiagnostic)
        {
            if (schemaSymbol == null)
                throw new ArgumentNullException(nameof(schemaSymbol));

            if (symbols == null)
                throw new ArgumentNullException(nameof(symbols));

            if (reportDiagnostic == null)
                throw new ArgumentNullException(nameof(reportDiagnostic));

            var properties =
                schemaSymbol.TypeKind == TypeKind.Interface
                    ? ParseInterface(schemaSymbol, symbols, reportDiagnostic)
                    : ParseEnum(schemaSymbol, symbols, reportDiagnostic);

            string namespaceName =
                schemaSymbol.ContainingNamespace == null ||
                schemaSymbol.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : schemaSymbol
                        .ContainingNamespace
                        .ToDisplayString();

            string fullyQualifiedName =
                schemaSymbol.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            return new SchemaModel(
                schemaSymbol.TypeKind == TypeKind.Interface,
                schemaSymbol,
                schemaSymbol.Name,
                namespaceName,
                fullyQualifiedName,
                properties);
        }

        private static IReadOnlyList<PropertyModel> ParseEnum(
            INamedTypeSymbol schemaSymbol,
            GeneratorSymbols symbols,
            Action<Diagnostic> reportDiagnostic)
        {
            var properties =
                new List<PropertyModel>();

            foreach (var field in
                     schemaSymbol
                         .GetMembers()
                         .OfType<IFieldSymbol>())
            {
                if (field.ConstantValue == null)
                    continue;

                var propertyTypeAttribute =
                    field.GetAttribute(
                        symbols.PropertyTypeAttribute);

                if (propertyTypeAttribute == null)
                    continue;

                var declaredType =
                    GetDeclaredType(
                        propertyTypeAttribute);

                if (declaredType == null)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.MissingType(
                            field));

                    continue;
                }

                var kind =
                    Classify(declaredType);

                if (kind == ValueKind.Unknown)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.UnsupportedType(
                            field,
                            declaredType));

                    continue;
                }

                var aggregateKind =
                    GetAggregateKind(
                        declaredType);

                bool isFlagsEnum =
                    IsFlagsEnum(
                        declaredType,
                        symbols.FlagsAttribute);

                ParseGroup(
                    field,
                    symbols,
                    out bool hasGroupAttribute,
                    out string groupName);

                ParseGroupComponent(
                    field,
                    symbols,
                    out bool hasGroupComponentAttribute,
                    out GroupComponentKind? groupComponent);

                string idExpression =
                    "(int)" +
                    schemaSymbol.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat) +
                    "." +
                    IdentifierUtility.EscapeIdentifier(
                        field.Name);

                properties.Add(
                    new PropertyModel(
                        field,
                        field.Name,
                        declaredType,
                        kind,
                        aggregateKind,
                        isFlagsEnum,
                        hasGroupAttribute,
                        groupName,
                        hasGroupComponentAttribute,
                        groupComponent,
                        idExpression,
                        Array.Empty<int>(),
                        emitScalarApi: true));
            }

            return properties;
        }

        private static IReadOnlyList<PropertyModel> ParseInterface(
            INamedTypeSymbol schemaSymbol,
            GeneratorSymbols symbols,
            Action<Diagnostic> reportDiagnostic)
        {
            var properties =
                new List<PropertyModel>();

            foreach (var property in
                     schemaSymbol
                         .GetMembers()
                         .OfType<IPropertySymbol>())
            {
                if (property.IsStatic)
                    continue;

                var propertyAttribute =
                    property.GetAttribute(
                        symbols.PropertyAttribute);

                if (propertyAttribute == null)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.MissingPropertyAttribute(
                            property));

                    continue;
                }

                var declaredType =
                    property.Type;

                var kind =
                    Classify(declaredType);

                if (kind == ValueKind.Unknown)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.UnsupportedType(
                            property,
                            declaredType));

                    continue;
                }

                var ids =
                    GetPropertyIds(
                        propertyAttribute);

                var aggregateKind =
                    GetAggregateKind(
                        declaredType);

                bool isFlagsEnum =
                    IsFlagsEnum(
                        declaredType,
                        symbols.FlagsAttribute);

                properties.Add(
                    new PropertyModel(
                        property,
                        property.Name,
                        declaredType,
                        kind,
                        aggregateKind,
                        isFlagsEnum,
                        hasGroupAttribute: false,
                        groupName: null,
                        hasGroupComponentAttribute: false,
                        groupComponent: null,
                        idExpression: ids.Count == 0 ? "0" : ids[0].ToString(),
                        storageIds: ids,
                        emitScalarApi: aggregateKind == AggregateKind.None));
            }

            return properties;
        }

        private static IReadOnlyList<int> GetPropertyIds(
            AttributeData propertyAttribute)
        {
            if (propertyAttribute.ConstructorArguments.Length == 0)
                return Array.Empty<int>();

            var argument =
                propertyAttribute.ConstructorArguments[0];

            if (argument.Kind != TypedConstantKind.Array)
                return Array.Empty<int>();

            return argument
                .Values
                .Where(value => value.Value is int)
                .Select(value => (int)value.Value)
                .ToArray();
        }

        private static ITypeSymbol GetDeclaredType(
            AttributeData propertyTypeAttribute)
        {
            foreach (var argument in
                     propertyTypeAttribute.ConstructorArguments)
            {
                if (argument.Kind == TypedConstantKind.Type &&
                    argument.Value is ITypeSymbol typeSymbol)
                {
                    return typeSymbol;
                }
            }

            return null;
        }

        private static void ParseGroup(
            IFieldSymbol field,
            GeneratorSymbols symbols,
            out bool hasAttribute,
            out string groupName)
        {
            var attribute =
                field.GetAttribute(
                    symbols.GroupAttribute);

            hasAttribute =
                attribute != null;

            groupName = null;

            if (attribute == null ||
                attribute.ConstructorArguments.Length == 0)
            {
                return;
            }

            if (attribute.ConstructorArguments[0].Value
                is string rawName)
            {
                // Keep empty/whitespace as empty.
                //
                // DP-030 must be able to distinguish:
                //
                // no [Group]
                //
                // from:
                //
                // [Group("")]
                groupName = rawName.Trim();
            }
        }

        private static void ParseGroupComponent(
            IFieldSymbol field,
            GeneratorSymbols symbols,
            out bool hasAttribute,
            out GroupComponentKind? component)
        {
            var attribute =
                field.GetAttribute(
                    symbols.GroupComponentAttribute);

            hasAttribute =
                attribute != null;

            component = null;

            if (attribute == null ||
                attribute.ConstructorArguments.Length == 0)
            {
                return;
            }

            if (!TryGetEnumMemberName(
                    attribute.ConstructorArguments[0],
                    out string componentName))
            {
                return;
            }

            switch (componentName)
            {
                case "X":
                    component = GroupComponentKind.X;
                    break;

                case "Y":
                    component = GroupComponentKind.Y;
                    break;

                case "Z":
                    component = GroupComponentKind.Z;
                    break;

                case "W":
                    component = GroupComponentKind.W;
                    break;

                case "R":
                    component = GroupComponentKind.R;
                    break;

                case "G":
                    component = GroupComponentKind.G;
                    break;

                case "B":
                    component = GroupComponentKind.B;
                    break;

                case "A":
                    component = GroupComponentKind.A;
                    break;
            }
        }

        private static bool TryGetEnumMemberName(
            TypedConstant constant,
            out string name)
        {
            name = null;

            if (!(constant.Type is INamedTypeSymbol enumType) ||
                enumType.TypeKind != TypeKind.Enum ||
                constant.Value == null)
            {
                return false;
            }

            foreach (var field in
                     enumType
                         .GetMembers()
                         .OfType<IFieldSymbol>())
            {
                if (!field.HasConstantValue)
                    continue;

                if (!Equals(
                        field.ConstantValue,
                        constant.Value))
                {
                    continue;
                }

                name = field.Name;
                return true;
            }

            return false;
        }

        private static ValueKind Classify(
            ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int32:
                    return ValueKind.Int32;

                case SpecialType.System_Single:
                    return ValueKind.Single;

                case SpecialType.System_Boolean:
                    return ValueKind.Boolean;

                case SpecialType.System_Int64:
                    return ValueKind.Int64;

                case SpecialType.System_Double:
                    return ValueKind.Double;
            }

            string fqn =
                type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            if (fqn == "global::System.DateTime")
                return ValueKind.DateTime;

            if (fqn == "global::System.TimeSpan")
                return ValueKind.TimeSpan;

            if (fqn == "global::UnityEngine.Vector2" ||
                fqn == "global::UnityEngine.Vector3" ||
                fqn == "global::UnityEngine.Vector4" ||
                fqn == "global::UnityEngine.Color")
            {
                // Aggregate components are stored as floats.
                return ValueKind.Single;
            }

            if (type.TypeKind == TypeKind.Enum)
                return ValueKind.Enum;

            return ValueKind.Unknown;
        }

        private static AggregateKind GetAggregateKind(
            ITypeSymbol type)
        {
            string fqn =
                type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            switch (fqn)
            {
                case "global::UnityEngine.Vector2":
                    return AggregateKind.Vector2;

                case "global::UnityEngine.Vector3":
                    return AggregateKind.Vector3;

                case "global::UnityEngine.Vector4":
                    return AggregateKind.Vector4;

                case "global::UnityEngine.Color":
                    return AggregateKind.Color;

                default:
                    return AggregateKind.None;
            }
        }

        private static bool IsFlagsEnum(
            ITypeSymbol type,
            INamedTypeSymbol flagsAttribute)
        {
            if (!(type is INamedTypeSymbol namedType))
                return false;

            if (namedType.TypeKind != TypeKind.Enum)
                return false;

            return namedType.HasAttribute(
                flagsAttribute);
        }
    }
}
