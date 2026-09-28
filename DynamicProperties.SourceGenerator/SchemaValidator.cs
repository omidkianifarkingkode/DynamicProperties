using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SchemaValidator
    {
        public const int MaxLogicalId = int.MaxValue >> 3;

        public static bool Validate(
            SchemaModel schema,
            Action<Diagnostic> reportDiagnostic)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (reportDiagnostic == null)
                throw new ArgumentNullException(nameof(reportDiagnostic));

            bool valid = true;

            var ids =
                new Dictionary<int, PropertyModel>();

            foreach (var property in schema.Properties)
            {
                if (property.LogicalId <= 0 ||
                    property.LogicalId > MaxLogicalId)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.InvalidLogicalId(
                            property,
                            MaxLogicalId));

                    valid = false;
                    continue;
                }

                if (ids.TryGetValue(
                        property.LogicalId,
                        out var existing))
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.DuplicateLogicalId(
                            existing,
                            property));

                    valid = false;
                    continue;
                }

                ids[property.LogicalId] = property;
            }

            return valid;
        }
    }
}
