using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class GeneratedNameValidator
    {
        private sealed class Origin
        {
            public string Description { get; }

            public IFieldSymbol Symbol { get; }

            public Origin(
                string description,
                IFieldSymbol symbol)
            {
                Description = description;
                Symbol = symbol;
            }
        }

        public static bool Validate(
            SchemaModel schema,
            IReadOnlyList<GroupModel> groups,
            Action<Diagnostic> reportDiagnostic)
        {
            var names =
                new Dictionary<string, Origin>(
                    StringComparer.Ordinal);

            bool valid = true;

            foreach (var property in schema.Properties)
            {
                string baseName =
                    IdentifierUtility.ToPascalIdentifier(
                        property.Name);

                string origin =
                    $"property '{property.Name}'";

                valid &=
                    AddPropertyMethods(
                        schema,
                        property,
                        baseName,
                        origin,
                        names,
                        reportDiagnostic);

                if (property.Kind == ValueKind.Enum &&
                    property.IsFlagsEnum)
                {
                    valid &=
                        AddFlagMethods(
                            schema,
                            property,
                            baseName,
                            names,
                            reportDiagnostic);
                }
            }

            foreach (var group in groups)
            {
                string baseName =
                    IdentifierUtility.ToPascalIdentifier(
                        group.Name);

                string origin =
                    $"group '{group.Name}'";

                IFieldSymbol symbol =
                    group.Components[0].Symbol;

                valid &=
                    AddMethod(
                        schema,
                        symbol,
                        baseName,
                        origin,
                        names,
                        reportDiagnostic);

                valid &=
                    AddMethod(
                        schema,
                        symbol,
                        "TryGet" + baseName,
                        origin,
                        names,
                        reportDiagnostic);

                valid &=
                    AddMethod(
                        schema,
                        symbol,
                        "Has" + baseName,
                        origin,
                        names,
                        reportDiagnostic);

                valid &=
                    AddMethod(
                        schema,
                        symbol,
                        "Set" + baseName,
                        origin,
                        names,
                        reportDiagnostic);

                valid &=
                    AddMethod(
                        schema,
                        symbol,
                        "Remove" + baseName,
                        origin,
                        names,
                        reportDiagnostic);
            }

            return valid;
        }

        private static bool AddPropertyMethods(
            SchemaModel schema,
            PropertyModel property,
            string baseName,
            string origin,
            Dictionary<string, Origin> names,
            Action<Diagnostic> reportDiagnostic)
        {
            bool valid = true;

            valid &=
                AddMethod(
                    schema,
                    property.Symbol,
                    baseName,
                    origin,
                    names,
                    reportDiagnostic);

            valid &=
                AddMethod(
                    schema,
                    property.Symbol,
                    "TryGet" + baseName,
                    origin,
                    names,
                    reportDiagnostic);

            valid &=
                AddMethod(
                    schema,
                    property.Symbol,
                    "Has" + baseName,
                    origin,
                    names,
                    reportDiagnostic);

            valid &=
                AddMethod(
                    schema,
                    property.Symbol,
                    "Set" + baseName,
                    origin,
                    names,
                    reportDiagnostic);

            valid &=
                AddMethod(
                    schema,
                    property.Symbol,
                    "Remove" + baseName,
                    origin,
                    names,
                    reportDiagnostic);

            return valid;
        }

        private static bool AddFlagMethods(
            SchemaModel schema,
            PropertyModel property,
            string propertyGeneratedName,
            Dictionary<string, Origin> names,
            Action<Diagnostic> reportDiagnostic)
        {
            bool valid = true;

            foreach (var flag in
                     property.DeclaredType
                         .GetMembers()
                         .OfType<IFieldSymbol>())
            {
                if (!flag.HasConstantValue)
                    continue;

                string methodName =
                    IdentifierUtility.GetFlagMethodName(
                        propertyGeneratedName,
                        flag.Name);

                string origin =
                    $"flag '{flag.Name}' of property '{property.Name}'";

                valid &=
                    AddMethod(
                        schema,
                        property.Symbol,
                        methodName,
                        origin,
                        names,
                        reportDiagnostic);
            }

            return valid;
        }

        private static bool AddMethod(
            SchemaModel schema,
            IFieldSymbol symbol,
            string methodName,
            string description,
            Dictionary<string, Origin> names,
            Action<Diagnostic> reportDiagnostic)
        {
            if (!names.TryGetValue(
                    methodName,
                    out var existing))
            {
                names[methodName] =
                    new Origin(
                        description,
                        symbol);

                return true;
            }

            reportDiagnostic(
                GeneratorDiagnostics.MethodCollision(
                    symbol,
                    methodName,
                    existing.Description,
                    description,
                    schema.FullyQualifiedName));

            return false;
        }
    }
}