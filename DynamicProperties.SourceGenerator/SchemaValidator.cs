using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SchemaValidator
    {
        public static IReadOnlyList<GroupModel> BuildValidGroups(
            SchemaModel schema,
            Action<Diagnostic> reportDiagnostic)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (reportDiagnostic == null)
                throw new ArgumentNullException(nameof(reportDiagnostic));

            var groups =
                new Dictionary<
                    string,
                    List<PropertyModel>>(
                    StringComparer.OrdinalIgnoreCase);

            //
            // First pass:
            // validate basic Group / GroupComponent usage
            // and collect named groups.
            //

            foreach (var property in schema.Properties)
            {
                if (property.HasGroupComponentAttribute &&
                    !property.HasGroupAttribute)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.ComponentWithoutGroup(
                            property));
                }

                if (!property.HasGroupAttribute)
                    continue;

                if (string.IsNullOrWhiteSpace(
                        property.GroupName))
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.EmptyGroup(
                            property));

                    continue;
                }

                if (!groups.TryGetValue(
                        property.GroupName,
                        out var members))
                {
                    members =
                        new List<PropertyModel>();

                    groups[property.GroupName] =
                        members;
                }

                members.Add(property);
            }

            var result =
                new List<GroupModel>();

            foreach (var pair in groups)
            {
                ValidateGroup(
                    pair.Key,
                    pair.Value,
                    reportDiagnostic,
                    result);
            }

            return result;
        }

        private static void ValidateGroup(
            string groupName,
            List<PropertyModel> members,
            Action<Diagnostic> reportDiagnostic,
            List<GroupModel> result)
        {
            bool hasErrors = false;

            //
            // All members must declare an aggregate PropertyType.
            //

            foreach (var member in members)
            {
                if (member.AggregateKind !=
                    AggregateKind.None)
                {
                    continue;
                }

                reportDiagnostic(
                    GeneratorDiagnostics.NonAggregateGroupMember(
                        member));

                hasErrors = true;
            }

            if (hasErrors)
                return;

            //
            // Every member in one group must use the same
            // aggregate type.
            //

            var kinds =
                members
                    .Select(x => x.AggregateKind)
                    .Distinct()
                    .ToArray();

            if (kinds.Length != 1)
            {
                string kindNames =
                    string.Join(
                        ", ",
                        kinds.Select(x => x.ToString()));

                reportDiagnostic(
                    GeneratorDiagnostics.MixedTypes(
                        members[0],
                        groupName,
                        kindNames));

                return;
            }

            AggregateKind kind =
                kinds[0];

            var requiredComponents =
                GroupContract.GetRequiredComponents(
                    kind);

            //
            // Every group member must explicitly identify
            // its component.
            //

            foreach (var member in members)
            {
                if (!member.HasGroupComponentAttribute)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.MissingComponent(
                            member));

                    hasErrors = true;

                    continue;
                }

                if (!member.GroupComponent.HasValue)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.InvalidComponentValue(
                            member));

                    hasErrors = true;
                }
            }

            if (hasErrors)
                return;

            //
            // Component must belong to the aggregate contract.
            //

            string expected =
                string.Join(
                    ", ",
                    requiredComponents.Select(
                        x => x.ToString()));

            foreach (var member in members)
            {
                var component =
                    member.GroupComponent.Value;

                if (GroupContract.IsComponentValid(
                        kind,
                        component))
                {
                    continue;
                }

                reportDiagnostic(
                    GeneratorDiagnostics.InvalidComponent(
                        member,
                        kind,
                        expected));

                hasErrors = true;
            }

            if (hasErrors)
                return;

            //
            // Components must be unique.
            //

            var componentGroups =
                members
                    .GroupBy(
                        x => x.GroupComponent.Value)
                    .ToArray();

            foreach (var componentGroup in
                     componentGroups)
            {
                var duplicates =
                    componentGroup
                        .Skip(1)
                        .ToArray();

                if (duplicates.Length == 0)
                    continue;

                foreach (var duplicate in duplicates)
                {
                    reportDiagnostic(
                        GeneratorDiagnostics.DuplicateComponent(
                            duplicate,
                            groupName,
                            componentGroup.Key));
                }

                hasErrors = true;
            }

            if (hasErrors)
                return;

            //
            // Exact required component set.
            //

            var mappedComponents =
                new HashSet<GroupComponentKind>(
                    members.Select(
                        x => x.GroupComponent.Value));

            foreach (var required in
                     requiredComponents)
            {
                if (mappedComponents.Contains(required))
                    continue;

                reportDiagnostic(
                    GeneratorDiagnostics.MissingRequiredComponent(
                        members[0],
                        groupName,
                        kind,
                        required));

                hasErrors = true;
            }

            if (hasErrors)
                return;

            //
            // Canonical order:
            //
            // Vector2 -> X,Y
            // Vector3 -> X,Y,Z
            // Vector4 -> X,Y,Z,W
            // Color   -> R,G,B,A
            //

            var ordered =
                requiredComponents
                    .Select(required =>
                        members.Single(
                            member =>
                                member.GroupComponent.Value ==
                                required))
                    .ToArray();

            result.Add(
                new GroupModel(
                    groupName,
                    kind,
                    ordered));
        }
    }
}