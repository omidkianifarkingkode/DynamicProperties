using System;

namespace DynamicProperty.SourceGen
{
    internal static class AggregateContract
    {
        public static int GetSlotCount(AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.None:
                    return 1;

                case AggregateKind.Vector2:
                    return 2;

                case AggregateKind.Vector3:
                    return 3;

                case AggregateKind.Vector4:
                case AggregateKind.Color:
                    return 4;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported aggregate kind '{kind}'.");
            }
        }

        public static string GetAggregateTypeName(AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.Vector2:
                    return "global::UnityEngine.Vector2";

                case AggregateKind.Vector3:
                    return "global::UnityEngine.Vector3";

                case AggregateKind.Vector4:
                    return "global::UnityEngine.Vector4";

                case AggregateKind.Color:
                    return "global::UnityEngine.Color";

                default:
                    throw new InvalidOperationException(
                        $"Unsupported aggregate kind '{kind}'.");
            }
        }

        public static string[] GetComponentAccessors(AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.Vector2:
                    return new[] { "x", "y" };

                case AggregateKind.Vector3:
                    return new[] { "x", "y", "z" };

                case AggregateKind.Vector4:
                    return new[] { "x", "y", "z", "w" };

                case AggregateKind.Color:
                    return new[] { "r", "g", "b", "a" };

                default:
                    throw new InvalidOperationException(
                        $"Unsupported aggregate kind '{kind}'.");
            }
        }

        public static string GetStorageIdExpression(int logicalId, int slot)
        {
            return "global::DynamicProperty.PropertyStorageId.Encode(" +
                logicalId.ToString() +
                ", " +
                slot.ToString() +
                ")";
        }
    }
}
