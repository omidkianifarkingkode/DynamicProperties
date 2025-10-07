using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PropertyPrinter
{
    /// <summary>
    /// Builds a multi-line string report for 32/64-bit properties.
    /// Safe to call from any ScriptableObject/MonoBehaviour.
    /// </summary>
    public static string BuildReport(
        string title,
        IEnumerable<Property32> props32,
        IEnumerable<Property64> props64)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(title))
            sb.AppendLine(title);

        // ---- 32-bit ----
        if (props32 == null)
        {
            sb.AppendLine("  [32] (null)");
        }
        else
        {
            bool any = false;
            foreach (var p in props32) { any = true; break; }
            if (!any)
            {
                sb.AppendLine("  [32] (none)");
            }
            else
            {
                sb.AppendLine("  [32]");
                foreach (var p in props32)
                    sb.AppendLine("    - " + Format32(in p));
            }
        }

        // ---- 64-bit ----
        if (props64 == null)
        {
            sb.AppendLine("  [64] (null)");
        }
        else
        {
            bool any = false;
            foreach (var p in props64) { any = true; break; }
            if (!any)
            {
                sb.AppendLine("  [64] (none)");
            }
            else
            {
                sb.AppendLine("  [64]");
                foreach (var p in props64)
                    sb.AppendLine("    - " + Format64(in p));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convenience: build and Debug.Log the report. Context helps ping the asset in console.
    /// </summary>
    public static void LogReport(
        UnityEngine.Object context,
        IEnumerable<Property32> props32,
        IEnumerable<Property64> props64,
        string title = null)
    {
        var report = BuildReport(title, props32, props64);
        Debug.Log(report, context);
    }

    // ---------- FORMATTERS (reused by any asset) ----------

    private static string Format32(in Property32 p)
    {
        var meta = PropertyMetadataCache.Get(p.Key);
        string label = string.IsNullOrEmpty(meta.DisplayName) ? p.Key.ToString() : meta.DisplayName;

        if (p.Key.Equals(default(PropertyKey)))
            return $"{label}: <no key>";

        switch (meta.Type)
        {
            case PropertyType.Float: return $"{label}: {p.GetFloat()} (raw {p.RawValue})";
            case PropertyType.Int: return $"{label}: {p.GetInt()} (raw {p.RawValue})";
            case PropertyType.Bool: return $"{label}: {p.GetBool()} (raw {p.RawValue})";
            case PropertyType.Enum: return $"{label}: {FormatEnum(meta.EnumType, p.GetInt())} (raw {p.RawValue})";
            default: return $"{label}: <unknown type {meta.Type}> (raw {p.RawValue})";
        }
    }

    private static string Format64(in Property64 p)
    {
        var meta = PropertyMetadataCache.Get(p.Key);
        string label = string.IsNullOrEmpty(meta.DisplayName) ? p.Key.ToString() : meta.DisplayName;

        if (p.Key.Equals(default(PropertyKey)))
            return $"{label}: <no key>";

        switch (meta.Type)
        {
            case PropertyType.Float: return $"{label}: {p.GetFloat()} (raw {p.RawValue})";
            case PropertyType.Int: return $"{label}: {p.GetInt()} (raw {p.RawValue})";
            case PropertyType.Bool: return $"{label}: {p.GetBool()} (raw {p.RawValue})";
            case PropertyType.Double: return $"{label}: {p.GetDouble()} (raw {p.RawValue})";
            case PropertyType.Long: return $"{label}: {p.RawValue}";
            case PropertyType.Enum: return $"{label}: {FormatEnum(meta.EnumType, p.GetInt())} (raw {p.GetInt()})";
            case PropertyType.DateTime: // ticks
                {
                    var ticks = p.RawValue;
                    DateTime dt;
                    try { dt = new DateTime(ticks, DateTimeKind.Utc); } catch { dt = DateTime.UnixEpoch; }
                    return $"{label}: {dt:yyyy-MM-dd HH:mm:ss} UTC (ticks {ticks})";
                }
            case PropertyType.TimeSpan: // ticks
                {
                    var ticks = p.RawValue;
                    TimeSpan ts;
                    try { ts = new TimeSpan(ticks); } catch { ts = TimeSpan.Zero; }
                    return $"{label}: {FormatTimeSpan(ts)} (ticks {ticks})";
                }
            default: return $"{label}: <unknown type {meta.Type}> (raw {p.RawValue})";
        }
    }

    private static string FormatEnum(Type enumType, int raw)
    {
        if (enumType == null) return $"(no enum) #{raw}";
        try
        {
            var val = Enum.ToObject(enumType, raw);
            return val.ToString(); // prints Flags combos as "A, B"
        }
        catch
        {
            return $"(invalid {enumType.Name}) #{raw}";
        }
    }

    private static string FormatTimeSpan(TimeSpan ts, bool shortForm = false)
    {
        return shortForm
            ? (ts.Days != 0 ? ts.ToString(@"d\.hh\:mm") : ts.ToString(@"hh\:mm"))
            : (ts.Days != 0 ? ts.ToString(@"d\.hh\:mm\:ss") : ts.ToString(@"hh\:mm\:ss"));
    }
}
