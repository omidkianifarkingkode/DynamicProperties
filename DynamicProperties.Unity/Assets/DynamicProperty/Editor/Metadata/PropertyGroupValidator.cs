using DynamicProperty.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicProperty.Editor
{
    internal static class PropertyGroupValidator
    {
        private static readonly PropertyComponent[] Vector2Components =
        {
            PropertyComponent.X,
            PropertyComponent.Y
        };

        private static readonly PropertyComponent[] Vector3Components =
        {
            PropertyComponent.X,
            PropertyComponent.Y,
            PropertyComponent.Z
        };

        private static readonly PropertyComponent[] Vector4Components =
        {
            PropertyComponent.X,
            PropertyComponent.Y,
            PropertyComponent.Z,
            PropertyComponent.W
        };

        private static readonly PropertyComponent[] ColorComponents =
        {
            PropertyComponent.R,
            PropertyComponent.G,
            PropertyComponent.B,
            PropertyComponent.A
        };

        public static IReadOnlyDictionary<string, PropertyGroupDefinition> Build(IPropertyMetadataResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            var membersByGroup = new Dictionary<string, List<(int id, PropertyMetadata meta)>>(StringComparer.OrdinalIgnoreCase);

            foreach (int id in resolver.GetAllValues().Distinct())
            {
                var meta = resolver.Get(id);

                if (meta == null || string.IsNullOrWhiteSpace(meta.GroupName))
                    continue;

                string groupName = meta.GroupName.Trim();

                if (!membersByGroup.TryGetValue(groupName, out var members))
                {
                    members = new List<(int, PropertyMetadata)>();

                    membersByGroup[groupName] = members;
                }

                members.Add((id, meta));
            }

            var definitions = new Dictionary<string, PropertyGroupDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in membersByGroup)
                definitions[pair.Key] = BuildDefinition(pair.Key, pair.Value);

            return definitions;
        }

        public static int GetRequiredComponentCount(PropertyGroupKind kind)
        {
            return GetRequiredComponents(kind).Count;
        }

        public static IReadOnlyList<PropertyComponent> GetRequiredComponents(PropertyGroupKind kind)
        {
            switch (kind)
            {
                case PropertyGroupKind.Vector2:
                    return Vector2Components;

                case PropertyGroupKind.Vector3:
                    return Vector3Components;

                case PropertyGroupKind.Vector4:
                    return Vector4Components;

                case PropertyGroupKind.Color:
                    return ColorComponents;

                default:
                    return Array.Empty<PropertyComponent>();
            }
        }

        private static PropertyGroupDefinition BuildDefinition(string groupName, List<(int id, PropertyMetadata meta)> members)
        {
            var originalMemberIds = members
                    .Select(x => x.id)
                    .Distinct()
                    .ToArray();

            var kinds = members
                    .Select(x => x.meta.GroupKind)
                    .Distinct()
                    .ToArray();

            //
            // DP-026 validation
            //

            if (members.Any(x => x.meta.GroupKind == PropertyGroupKind.None))
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' contains a property that does not declare a supported aggregate type.");
            }

            if (kinds.Length != 1)
            {
                string kindNames = string.Join(", ", kinds.Select(x => x.ToString()));

                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' contains mixed aggregate types: {kindNames}.");
            }

            PropertyGroupKind kind = kinds[0];

            if (members.Any(x => x.meta.Type != PropertyValueType.Float))
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' contains a component that is not stored as a float.");
            }

            var requiredComponents = GetRequiredComponents(kind);

            if (requiredComponents.Count == 0)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' uses unsupported aggregate type '{kind}'.");
            }

            if (originalMemberIds.Length != requiredComponents.Count)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' is {kind} and requires exactly {requiredComponents.Count} components, but {originalMemberIds.Length} were defined.");
            }

            //
            // DP-027 validation
            //

            var missingMappings = members
                    .Where(x => !x.meta.GroupComponent.HasValue)
                    .Select(x => x.id)
                    .ToArray();

            if (missingMappings.Length > 0)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' has members without [GroupComponent]: {string.Join(", ", missingMappings)}.");
            }

            var mappedComponents = members
                    .Select(x => x.meta.GroupComponent.Value)
                    .ToArray();

            var invalidComponents = mappedComponents
                    .Where(component => !requiredComponents.Contains(component))
                    .Distinct()
                    .ToArray();

            if (invalidComponents.Length > 0)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' contains invalid components for {kind}: {string.Join(", ", invalidComponents)}.");
            }

            var duplicateComponents = members
                    .GroupBy(x => x.meta.GroupComponent.Value)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToArray();

            if (duplicateComponents.Length > 0)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' defines duplicate components: {string.Join(", ", duplicateComponents)}.");
            }

            var missingComponents = requiredComponents
                    .Where(required => !mappedComponents.Contains(required))
                    .ToArray();

            if (missingComponents.Length > 0)
            {
                return Invalid(
                    groupName,
                    originalMemberIds,
                    $"Group '{groupName}' is missing required components: {string.Join(", ", missingComponents)}.");
            }

            //
            // Canonical component order.
            //
            // This is the important DP-027 result:
            // MemberIds is no longer schema/name order.
            // It is X,Y,Z,W or R,G,B,A order.
            //

            var orderedMemberIds = requiredComponents
                    .Select(component => members.Single(x => x.meta.GroupComponent.Value == component).id)
                    .ToArray();

            return new PropertyGroupDefinition(groupName, kind, orderedMemberIds, null);
        }

        private static PropertyGroupDefinition Invalid(string groupName, IReadOnlyList<int> memberIds, string error)
        {
            return new PropertyGroupDefinition(groupName, PropertyGroupKind.None, memberIds, error);
        }
    }
}