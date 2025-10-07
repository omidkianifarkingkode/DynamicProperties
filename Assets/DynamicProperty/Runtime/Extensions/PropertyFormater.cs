using System;

public static class PropertyFormater
{
    public static string ToDebugString(this in Property32 p)
    {
        var t = p.GetValueType();
        return t switch
        {
            PropertyType.Float => $"{p.Key} (Float)  = {p.GetFloat()} [{p.RawValue}]",
            PropertyType.Int => $"{p.Key} (Int)    = {p.GetInt()} [{p.RawValue}]",
            PropertyType.Bool => $"{p.Key} (Bool)   = {p.GetBool()} [{p.RawValue}]",
            PropertyType.Enum => $"{p.Key} (Enum)   = {EnumLabel32(p)} [{p.RawValue}]",
            _ => $"{p.Key} = ? → {p.RawValue}"
        };
    }

    public static string ToDebugString(this in Property64 p)
    {
        var t = p.GetValueType();
        return t switch
        {
            PropertyType.Float => $"{p.Key} (Float)  = {p.GetFloat()} [{p.RawValue}]",
            PropertyType.Int => $"{p.Key} (Int)    = {p.GetInt()} [{p.RawValue}]",
            PropertyType.Bool => $"{p.Key} (Bool)   = {p.GetBool()} [{p.RawValue}]",
            PropertyType.Double => $"{p.Key} (Double) = {p.GetDouble()} [{p.RawValue}]",
            PropertyType.Long => $"{p.Key} (Long)   = {p.GetLong()}",
            PropertyType.Enum => $"{p.Key} (Enum)   = {EnumLabel64(p)} [{p.GetInt()}]",
            PropertyType.DateTime => $"{p.Key} (DateTime)= {new DateTime(p.RawValue, DateTimeKind.Utc):yyyy-MM-dd HH:mm:ss} UTC",
            PropertyType.TimeSpan => $"{p.Key} (TimeSpan)= {FormatTs(TimeSpan.FromSeconds(p.RawValue))}",
            _ => $"{p.Key} = ? → {p.RawValue}"
        };
    }

    private static string EnumLabel32(in Property32 p)
    {
        var meta = PropertyMetadataCache.Get(p.Key);
        if (meta.EnumType == null) return "(no enum type)";
        return Enum.ToObject(meta.EnumType, p.RawValue).ToString();
    }

    private static string EnumLabel64(in Property64 p)
    {
        var meta = PropertyMetadataCache.Get(p.Key);
        if (meta.EnumType == null) return "(no enum type)";
        return Enum.ToObject(meta.EnumType, p.GetInt()).ToString();
    }

    private static string FormatTs(TimeSpan ts)
        => ts.Days != 0 ? ts.ToString(@"d\.hh\:mm\:ss") : ts.ToString(@"hh\:mm\:ss");
}
