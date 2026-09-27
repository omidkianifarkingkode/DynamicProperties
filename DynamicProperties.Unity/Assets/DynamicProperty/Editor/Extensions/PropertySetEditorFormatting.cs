using System;
using System.Text;

namespace DynamicProperty.Editor.Extensions
{
    public static class PropertySetEditorFormatting
    {
        /// <summary>
        /// Returns a metadata-aware representation of a PropertySet.
        /// The schema must be explicitly provided because PropertySet itself
        /// is intentionally schema-agnostic.
        /// </summary>
        public static string ToPrettyString(this PropertySet set, Type schemaType, UnityEngine.Object context = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"PropertySet ({(context ? context.name : "no owner")})");

            if (set == null)
            {
                sb.AppendLine("  <null>");
                return sb.ToString();
            }

            if (!PropertyMetadataRegistry.TryGetResolver(schemaType, out var resolver, out var error))
            {
                sb.AppendLine($"  <invalid schema: {error}>");

                AppendRaw(set, sb);

                return sb.ToString();
            }

            Append32(set, resolver, sb);
            Append64(set, resolver, sb);

            return sb.ToString();
        }

        private static void Append32(PropertySet set, IPropertyMetadataResolver resolver, StringBuilder sb)
        {
            var list = set.Raw32;

            if (list == null || list.Count == 0)
            {
                sb.AppendLine("  [32] (none)");
                return;
            }

            sb.AppendLine("  [32]");

            foreach (var property in list)
            {
                var meta = resolver.Get(property.Id);

                string label =
                    meta?.DisplayName ??
                    resolver.GetName(property.Id) ??
                    $"ID {property.Id}";

                sb.AppendLine("    - " + Format32(property, label, meta));
            }
        }

        private static void Append64(PropertySet set, IPropertyMetadataResolver resolver, StringBuilder sb)
        {
            var list = set.Raw64;

            if (list == null || list.Count == 0)
            {
                sb.AppendLine("  [64] (none)");
                return;
            }

            sb.AppendLine("  [64]");

            foreach (var property in list)
            {
                var meta = resolver.Get(property.Id);

                string label =
                    meta?.DisplayName ??
                    resolver.GetName(property.Id) ??
                    $"ID {property.Id}";

                sb.AppendLine("    - " + Format64(property, label, meta));
            }
        }

        /// <summary>
        /// Fallback when schema metadata cannot be resolved.
        /// Raw data is still useful for diagnostics.
        /// </summary>
        private static void AppendRaw(PropertySet set, StringBuilder sb)
        {
            var list32 = set.Raw32;

            if (list32 == null || list32.Count == 0)
            {
                sb.AppendLine("  [32] (none)");
            }
            else
            {
                sb.AppendLine("  [32]");

                foreach (var property in list32)
                {
                    sb.AppendLine($"    - ID {property.Id}: raw {property.RawValue}");
                }
            }

            var list64 = set.Raw64;

            if (list64 == null || list64.Count == 0)
            {
                sb.AppendLine("  [64] (none)");
            }
            else
            {
                sb.AppendLine("  [64]");

                foreach (var property in list64)
                {
                    sb.AppendLine($"    - ID {property.Id}: raw {property.RawValue}");
                }
            }
        }

        private static string Format32(DynamicProperty32 property, string label, PropertyMetadata meta)
        {
            var type = meta?.Type ?? PropertyValueType.Int;

            var union = new ValueUnion32 { raw = property.RawValue };

            return type switch
            {
                PropertyValueType.Float =>
                    $"{label}: {union.asFloat} (raw {property.RawValue})",

                PropertyValueType.Int =>
                    $"{label}: {union.asInt} (raw {property.RawValue})",

                PropertyValueType.Bool =>
                    $"{label}: {union.asBool} (raw {property.RawValue})",

                PropertyValueType.Enum =>
                    FormatEnum32(property, label, meta),

                _ => $"{label}: <unknown 32 type {type}> (raw {property.RawValue})"
            };
        }

        private static string Format64(DynamicProperty64 property, string label, PropertyMetadata meta)
        {
            var type = meta?.Type ?? PropertyValueType.Long;

            var union = new ValueUnion64 { raw = property.RawValue };

            return type switch
            {
                PropertyValueType.Long => $"{label}: {union.raw}",

                PropertyValueType.Double => $"{label}: {union.asDouble} (raw {union.raw})",

                PropertyValueType.Enum => FormatEnum64(property, label, meta),

                PropertyValueType.DateTime => FormatUtcDateTime(label, union.raw),

                PropertyValueType.TimeSpan => $"{label}: {new TimeSpan(union.raw)} (ticks {union.raw})",

                _ => $"{label}: <unknown 64 type {type}> (raw {union.raw})"
            };
        }

        private static string FormatEnum32(DynamicProperty32 property, string label, PropertyMetadata meta)
        {
            if (meta?.EnumType == null)
            {
                return $"{label}: <enum?> (raw {property.RawValue})";
            }

            if (EnumBitUtility.Uses64BitStorage(meta.EnumType))
            {
                return $"{label}: <wrong 32-bit storage> (raw {property.RawValue})";
            }

            var value =
                EnumBitUtility.FromRaw32(meta.EnumType, property.RawValue);

            return $"{label}: {value} (raw {property.RawValue})";
        }

        private static string FormatEnum64(DynamicProperty64 property, string label, PropertyMetadata meta)
        {
            if (meta?.EnumType == null)
            {
                return $"{label}: <enum?> (raw {property.RawValue})";
            }

            if (!EnumBitUtility.Uses64BitStorage(meta.EnumType))
            {
                return $"{label}: <wrong 64-bit storage> (raw {property.RawValue})";
            }

            var value = EnumBitUtility.FromRaw64(meta.EnumType, property.RawValue);

            return $"{label}: {value} (raw {property.RawValue})";
        }

        private static string FormatUtcDateTime(string label, long ticks)
        {
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
            {
                return $"{label}: <invalid UTC DateTime> (ticks {ticks})";
            }

            var value = new DateTime(ticks, DateTimeKind.Utc);

            return $"{label}: {value:yyyy-MM-dd HH:mm:ss} UTC (ticks {ticks})";
        }
    }
}
