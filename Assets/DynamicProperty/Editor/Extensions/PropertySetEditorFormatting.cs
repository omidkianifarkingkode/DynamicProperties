using System;
using System.Text;

namespace DynamicProperty.Editor.Extensions
{
    public static class PropertySetEditorFormatting
    {
        /// Pretty, metadata-aware formatter for use in Editor only.
        public static string ToPrettyString(this DynamicProperty.PropertySet set, UnityEngine.Object context = null)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;

            var sb = new StringBuilder();
            sb.AppendLine($"PropertySet ({(context ? context.name : "no owner")})");

            if (set == null)
            {
                sb.AppendLine("  <null>");
                return sb.ToString();
            }

            // 32-bit
            var list32 = set.Raw32;
            if (list32 == null || list32.Count == 0) sb.AppendLine("  [32] (none)");
            else
            {
                sb.AppendLine("  [32]");
                foreach (var p in list32)
                {
                    var meta = resolver?.Get(p.Id);
                    string label = meta?.DisplayName ?? (resolver != null ? Enum.GetName(resolver.BoundEnumType, p.Id) : null) ?? $"ID {p.Id}";
                    sb.AppendLine("    - " + Format32(p, label, meta));
                }
            }

            // 64-bit
            var list64 = set.Raw64;
            if (list64 == null || list64.Count == 0) sb.AppendLine("  [64] (none)");
            else
            {
                sb.AppendLine("  [64]");
                foreach (var p in list64)
                {
                    var meta = resolver?.Get(p.Id);
                    string label = meta?.DisplayName ?? (resolver != null ? Enum.GetName(resolver.BoundEnumType, p.Id) : null) ?? $"ID {p.Id}";
                    sb.AppendLine("    - " + Format64(p, label, meta));
                }
            }

            return sb.ToString();
        }

        // --- same formatting helpers you already had (Editor side) ---
        private static string Format32(DynamicProperty32 p, string label, PropertyMetadata meta)
        {
            var type = meta?.Type ?? PropertyValueType.Int;
            var u = new ValueUnion32 { raw = p.RawValue };

            return type switch
            {
                PropertyValueType.Float => $"{label}: {u.asFloat} (raw {p.RawValue})",
                PropertyValueType.Int => $"{label}: {u.asInt} (raw {p.RawValue})",
                PropertyValueType.Bool => $"{label}: {u.asBool} (raw {p.RawValue})",
                PropertyValueType.Enum => meta?.EnumType != null
                    ? $"{label}: {Enum.ToObject(meta.EnumType, u.asInt)} (raw {p.RawValue})"
                    : $"{label}: <enum?> (raw {p.RawValue})",
                _ => $"{label}: <unknown 32 type {type}> (raw {p.RawValue})"
            };
        }

        private static string Format64(DynamicProperty64 p, string label, PropertyMetadata meta)
        {
            var type = meta?.Type ?? PropertyValueType.Long;
            var u = new ValueUnion64 { raw = p.RawValue };

            return type switch
            {
                PropertyValueType.Long => $"{label}: {u.raw}",
                PropertyValueType.Double => $"{label}: {u.asDouble} (raw {u.raw})",
                PropertyValueType.Int => $"{label}: {u.asInt} (raw {u.raw})",
                PropertyValueType.Float => $"{label}: {u.asFloat} (raw {u.raw})",
                PropertyValueType.Bool => $"{label}: {u.asBool} (raw {u.raw})",
                PropertyValueType.Enum => meta?.EnumType != null
                    ? $"{label}: {Enum.ToObject(meta.EnumType, u.asInt)} (raw {u.raw})"
                    : $"{label}: <enum?> (raw {u.raw})",
                PropertyValueType.DateTime => $"{label}: {new DateTime(u.raw, DateTimeKind.Utc):yyyy-MM-dd HH:mm:ss} UTC (ticks {u.raw})",
                PropertyValueType.TimeSpan => $"{label}: {new TimeSpan(u.raw)} (ticks {u.raw})",
                _ => $"{label}: <unknown 64 type {type}> (raw {u.raw})"
            };
        }
    }
}