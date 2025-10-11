using DynamicProperty;
using DynamicProperty.Editor;
using System;
using System.Text;

public static class PropertySetPrinter
    {
        /// <summary>
        /// Builds a human-readable string of all properties in a PropertySet.
        /// </summary>
        public static string Format(PropertySet set, UnityEngine.Object context = null)
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

            // 32-bit section
            var list32 = set.Raw32; // requires PropertySet.Raw32 to be exposed as IReadOnlyList<DynamicProperty32>
            if (list32 == null || list32.Count == 0)
            {
                sb.AppendLine("  [32] (none)");
            }
            else
            {
                sb.AppendLine("  [32]");
                for (int i = 0; i < list32.Count; i++)
                {
                    var p = list32[i];
                    var meta = resolver?.Get(p.id);
                    string label = meta?.DisplayName ?? (resolver != null ? Enum.GetName(resolver.BoundEnumType, p.id) : null) ?? $"ID {p.id}";
                    sb.AppendLine("    - " + Format32(p, label, meta));
                }
            }

            // 64-bit section
            var list64 = set.Raw64; // requires PropertySet.Raw64 to be exposed as IReadOnlyList<DynamicProperty64>
            if (list64 == null || list64.Count == 0)
            {
                sb.AppendLine("  [64] (none)");
            }
            else
            {
                sb.AppendLine("  [64]");
                for (int i = 0; i < list64.Count; i++)
                {
                    var p = list64[i];
                    var meta = resolver?.Get(p.id);
                    string label = meta?.DisplayName ?? (resolver != null ? Enum.GetName(resolver.BoundEnumType, p.id) : null) ?? $"ID {p.id}";
                    sb.AppendLine("    - " + Format64(p, label, meta));
                }
            }

            return sb.ToString();
        }

        // ---------- helpers ----------
        private static string Format32(in DynamicProperty32 p, string label, PropertyMetadata meta)
        {
            // Fallback if metadata is missing
            var type = meta?.Type ?? PropertyValueType.Int;

            // Use the raw value union for reads
            var u = new ValueUnion32 { raw = p.rawValue };

            switch (type)
            {
                case PropertyValueType.Float:
                    return $"{label}: {u.asFloat} (raw {p.rawValue})";

                case PropertyValueType.Int:
                    return $"{label}: {u.asInt} (raw {p.rawValue})";

                case PropertyValueType.Bool:
                    return $"{label}: {u.asBool} (raw {p.rawValue})";

                case PropertyValueType.Enum:
                    {
                        if (meta?.EnumType == null)
                            return $"{label}: <enum?> (raw {p.rawValue})";
                        object ev = Enum.ToObject(meta.EnumType, u.asInt);
                        return $"{label}: {ev} (raw {p.rawValue})";
                    }

                case PropertyValueType.DateTimeShort:
                    {
                        // minutes since Unix epoch (int)
                        try
                        {
                            var dt = DateTime.UnixEpoch.AddMinutes(u.asInt);
                            return $"{label}: {dt:yyyy-MM-dd HH:mm} (raw {p.rawValue} min)";
                        }
                        catch
                        {
                            return $"{label}: <invalid datetime> (raw {p.rawValue})";
                        }
                    }

                case PropertyValueType.TimeSpanShort:
                    {
                        // minutes stored as int
                        var ts = TimeSpan.FromMinutes(u.asInt);
                        return $"{label}: {ts} (raw {p.rawValue} min)";
                    }

                default:
                    return $"{label}: <unknown 32 type {type}> (raw {p.rawValue})";
            }
        }

        private static string Format64(in DynamicProperty64 p, string label, PropertyMetadata meta)
        {
            var type = meta?.Type ?? PropertyValueType.Long;
            var u = new ValueUnion64 { raw = p.rawValue };

            switch (type)
            {
                case PropertyValueType.Long:
                    return $"{label}: {u.raw}";

                case PropertyValueType.Double:
                    return $"{label}: {u.asDouble} (raw {u.raw})";

                case PropertyValueType.Int:
                    return $"{label}: {u.asInt} (raw {u.raw})";

                case PropertyValueType.Float:
                    return $"{label}: {u.asFloat} (raw {u.raw})";

                case PropertyValueType.Bool:
                    return $"{label}: {u.asBool} (raw {u.raw})";

                case PropertyValueType.Enum:
                    {
                        if (meta?.EnumType == null)
                            return $"{label}: <enum?> (raw {u.raw})";
                        object ev = Enum.ToObject(meta.EnumType, u.asInt);
                        return $"{label}: {ev} (raw {u.raw})";
                    }

                case PropertyValueType.DateTime:
                    {
                        try
                        {
                            var dt = new DateTime(u.raw, DateTimeKind.Utc);
                            return $"{label}: {dt:yyyy-MM-dd HH:mm:ss} UTC (ticks {u.raw})";
                        }
                        catch
                        {
                            return $"{label}: <invalid ticks {u.raw}>";
                        }
                    }

                case PropertyValueType.TimeSpan:
                    {
                        try
                        {
                            var ts = new TimeSpan(u.raw);
                            return $"{label}: {ts} (ticks {u.raw})";
                        }
                        catch
                        {
                            return $"{label}: <invalid timespan ticks {u.raw}>";
                        }
                    }

                default:
                    return $"{label}: <unknown 64 type {type}> (raw {u.raw})";
            }
        }
    }


