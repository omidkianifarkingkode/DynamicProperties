using System;

public static class Property64Extensions
{
    // ---- metadata ----
    public static PropertyType GetValueType(this in Property64 p)
        => PropertyMetadataCache.Get(p.Key).Type;

    // ---- unions ----
    private static ValueUnion64 U(this in Property64 p) => new ValueUnion64 { raw = p.RawValue };
    private static void SetRaw(ref Property64 p, long raw) => p.RawValue = raw;

    // ---- basic types ----
    public static int GetInt(this in Property64 p) => p.U().asInt;
    public static void SetInt(this ref Property64 p, int v) => SetRaw(ref p, new ValueUnion64 { asInt = v }.raw);

    public static float GetFloat(this in Property64 p) => p.U().asFloat;
    public static void SetFloat(this ref Property64 p, float v) => SetRaw(ref p, new ValueUnion64 { asFloat = v }.raw);

    public static bool GetBool(this in Property64 p) => p.U().asBool;
    public static void SetBool(this ref Property64 p, bool v) => SetRaw(ref p, new ValueUnion64 { asBool = v }.raw);

    public static double GetDouble(this in Property64 p) => p.U().asDouble;
    public static void SetDouble(this ref Property64 p, double v) => SetRaw(ref p, new ValueUnion64 { asDouble = v }.raw);

    public static long GetLong(this in Property64 p) => p.U().asLong;
    public static void SetLong(this ref Property64 p, long v) => SetRaw(ref p, new ValueUnion64 { asLong = v }.raw);

    // ---- time types (choose your policy; here: DateTime/TimeSpan as ticks in long) ----
    public static DateTime GetDateTime(this in Property64 p)
        => new DateTime(p.RawValue, DateTimeKind.Utc);

    public static void SetDateTime(this ref Property64 p, DateTime value)
        => SetRaw(ref p, value.Ticks);

    // unix millis helpers if you keep them:
    public static DateTime GetUnixMillis(this in Property64 p)
        => DateTimeOffset.FromUnixTimeMilliseconds(p.RawValue).UtcDateTime;

    public static void SetUnixMillis(this ref Property64 p, DateTime value)
        => SetRaw(ref p, new DateTimeOffset(value).ToUnixTimeMilliseconds());

    // timespan (here: store seconds in long — or switch to ticks if you prefer)
    public static TimeSpan GetTimeSpan(this in Property64 p)
        => TimeSpan.FromSeconds(p.RawValue);

    public static void SetTimeSpan(this ref Property64 p, TimeSpan value)
        => SetRaw(ref p, (long)Math.Round(value.TotalSeconds));

    // ---- enums (32-bit masks by default) ----
    public static TEnum GetEnum<TEnum>(this in Property64 p) where TEnum : struct, Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), p.U().asInt);

    public static void SetEnum<TEnum>(this ref Property64 p, TEnum value) where TEnum : struct, Enum
        => SetInt(ref p, Convert.ToInt32(value));

    public static bool HasFlag<TEnum>(this in Property64 p, TEnum flag) where TEnum : struct, Enum
        => (p.U().asInt & Convert.ToInt32(flag)) != 0;

    public static void AddFlag<TEnum>(this ref Property64 p, TEnum flag) where TEnum : struct, Enum
        => SetInt(ref p, p.U().asInt | Convert.ToInt32(flag));

    public static void RemoveFlag<TEnum>(this ref Property64 p, TEnum flag) where TEnum : struct, Enum
        => SetInt(ref p, p.U().asInt & ~Convert.ToInt32(flag));

    public static void ClearFlags(this ref Property64 p) => SetInt(ref p, 0);

    // NOTE: if you actually need 64-bit flag enums, duplicate the above using Convert.ToInt64 and SetLong.
}
