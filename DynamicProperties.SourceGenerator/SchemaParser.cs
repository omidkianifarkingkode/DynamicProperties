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
                schemaSymbol,
                schemaSymbol.Name,
                namespaceName,
                fullyQualifiedName,
                ParseInterface(schemaSymbol, symbols, reportDiagnostic));
        }

        private static IReadOnlyList<PropertyModel> ParseInterface(
            INamedTypeSymbol schemaSymbol,
            GeneratorSymbols symbols,
            Action<Diagnostic> reportDiagnostic)
        {
            var properties =
                new List<PropertyModel>();

            foreach (var inherited in schemaSymbol.Interfaces)
            {
                if (!SymbolEqualityComparer.Default.Equals(
                        inherited,
                        symbols.PropertySchema))
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.SchemaInheritance(
                            schemaSymbol,
                            inherited));
                }
            }

            foreach (var property in
                     schemaSymbol
                         .GetMembers()
                         .OfType<IPropertySymbol>())
            {
                if (property.IsIndexer)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.IndexerProperty(
                            property));

                    continue;
                }

                if (property.IsStatic)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.StaticProperty(
                            property));

                    continue;
                }

                if (property.SetMethod != null)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.SetterProperty(
                            property));

                    continue;
                }

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

                properties.Add(
                    new PropertyModel(
                        property,
                        property.Name,
                        declaredType,
                        kind,
                        GetAggregateKind(declaredType),
                        IsFlagsEnum(declaredType, symbols.FlagsAttribute),
                        GetPropertyId(propertyAttribute)));
            }

            return properties;
        }

        private static int GetPropertyId(
            AttributeData propertyAttribute)
        {
            if (propertyAttribute.ConstructorArguments.Length == 0)
                return 0;

            object value =
                propertyAttribute.ConstructorArguments[0].Value;

            return value is int id ? id : 0;
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

            if (GetAggregateKind(type) != AggregateKind.None)
                return ValueKind.Single;

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
