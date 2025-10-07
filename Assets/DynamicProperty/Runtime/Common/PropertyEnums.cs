using System;

public static class PropertyEnums
{
    // ----- 32-bit enum helpers -----
    public static TEnum GetEnum32<TEnum>(int raw) where TEnum : struct, Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), raw);

    public static int SetEnum32<TEnum>(TEnum value) where TEnum : struct, Enum
        => Convert.ToInt32(value);

    public static bool HasFlag32<TEnum>(int raw, TEnum flag) where TEnum : struct, Enum
        => (raw & Convert.ToInt32(flag)) != 0;

    public static int AddFlag32<TEnum>(int raw, TEnum flag) where TEnum : struct, Enum
        => raw | Convert.ToInt32(flag);

    public static int RemoveFlag32<TEnum>(int raw, TEnum flag) where TEnum : struct, Enum
        => raw & ~Convert.ToInt32(flag);

    // ----- 64-bit enum helpers (if you ever need true 64-bit flags) -----
    public static TEnum GetEnum64<TEnum>(long raw) where TEnum : struct, Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), raw);

    public static long SetEnum64<TEnum>(TEnum value) where TEnum : struct, Enum
        => Convert.ToInt64(value);

    public static bool HasFlag64<TEnum>(long raw, TEnum flag) where TEnum : struct, Enum
        => (raw & Convert.ToInt64(flag)) != 0L;

    public static long AddFlag64<TEnum>(long raw, TEnum flag) where TEnum : struct, Enum
        => raw | Convert.ToInt64(flag);

    public static long RemoveFlag64<TEnum>(long raw, TEnum flag) where TEnum : struct, Enum
        => raw & ~Convert.ToInt64(flag);
}
