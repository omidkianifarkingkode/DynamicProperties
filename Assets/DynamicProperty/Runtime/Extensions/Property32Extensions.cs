using System;

public static class Property32Extensions
{
    // ---- metadata ----
    public static PropertyType GetValueType(this in Property32 p)
        => PropertyMetadataCache.Get(p.Key).Type;

    // ---- unions ----
    private static ValueUnion32 U(this in Property32 p) => new ValueUnion32 { raw = p.rawValue };
    private static void SetRaw(ref Property32 p, int raw) => p.rawValue = raw;

    // ---- basic types ----
    public static int GetInt(this in Property32 p) => p.U().asInt;
    public static void SetInt(this ref Property32 p, int v) => SetRaw(ref p, new ValueUnion32 { asInt = v }.raw);

    public static float GetFloat(this in Property32 p) => p.U().asFloat;
    public static void SetFloat(this ref Property32 p, float v) => SetRaw(ref p, new ValueUnion32 { asFloat = v }.raw);

    public static bool GetBool(this in Property32 p) => p.U().asBool;
    public static void SetBool(this ref Property32 p, bool v) => SetRaw(ref p, new ValueUnion32 { asBool = v }.raw);

    // ---- enums ----
    public static TEnum GetEnum<TEnum>(this in Property32 p) where TEnum : struct, Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), p.U().asInt);

    public static void SetEnum<TEnum>(this ref Property32 p, TEnum value) where TEnum : struct, Enum
        => SetInt(ref p, Convert.ToInt32(value));

    // ---- flags ----
    public static bool HasFlag<TEnum>(this in Property32 p, TEnum flag) where TEnum : struct, Enum
        => (p.U().asInt & Convert.ToInt32(flag)) != 0;

    public static void AddFlag<TEnum>(this ref Property32 p, TEnum flag) where TEnum : struct, Enum
        => SetInt(ref p, p.U().asInt | Convert.ToInt32(flag));

    public static void RemoveFlag<TEnum>(this ref Property32 p, TEnum flag) where TEnum : struct, Enum
        => SetInt(ref p, p.U().asInt & ~Convert.ToInt32(flag));

    public static void ClearFlags(this ref Property32 p) => SetInt(ref p, 0);
}
