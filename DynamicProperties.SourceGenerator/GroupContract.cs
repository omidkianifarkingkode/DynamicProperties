using System;
using System.Collections.Generic;

namespace DynamicProperty.SourceGen
{
    internal static class GroupContract
    {
        private static readonly GroupComponentKind[] Vector2Components =
        {
            GroupComponentKind.X,
            GroupComponentKind.Y
        };

        private static readonly GroupComponentKind[] Vector3Components =
        {
            GroupComponentKind.X,
            GroupComponentKind.Y,
            GroupComponentKind.Z
        };

        private static readonly GroupComponentKind[] Vector4Components =
        {
            GroupComponentKind.X,
            GroupComponentKind.Y,
            GroupComponentKind.Z,
            GroupComponentKind.W
        };

        private static readonly GroupComponentKind[] ColorComponents =
        {
            GroupComponentKind.R,
            GroupComponentKind.G,
            GroupComponentKind.B,
            GroupComponentKind.A
        };

        public static IReadOnlyList<GroupComponentKind> GetRequiredComponents(
            AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.Vector2:
                    return Vector2Components;

                case AggregateKind.Vector3:
                    return Vector3Components;

                case AggregateKind.Vector4:
                    return Vector4Components;

                case AggregateKind.Color:
                    return ColorComponents;

                default:
                    return Array.Empty<GroupComponentKind>();
            }
        }

        public static bool IsComponentValid(
            AggregateKind kind,
            GroupComponentKind component)
        {
            var required =
                GetRequiredComponents(kind);

            for (int i = 0; i < required.Count; i++)
            {
                if (required[i] == component)
                    return true;
            }

            return false;
        }
    }
}