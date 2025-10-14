using System;

namespace DynamicProperty
{
    public static class DynamicProperty64Extensions
    {
        private static ValueUnion64 U(this in DynamicProperty64 p) => new ValueUnion64 { raw = p.RawValue };
        private static void SetRaw(ref DynamicProperty64 p, long raw) => p.RawValue = raw;

        public static int GetInt(this in DynamicProperty64 p) => p.U().asInt;
        public static void SetInt(this ref DynamicProperty64 p, int v) => SetRaw(ref p, new ValueUnion64 { asInt = v }.raw);

        public static float GetFloat(this in DynamicProperty64 p) => p.U().asFloat;
        public static void SetFloat(this ref DynamicProperty64 p, float v) => SetRaw(ref p, new ValueUnion64 { asFloat = v }.raw);

        public static bool GetBool(this in DynamicProperty64 p) => p.U().asBool;
        public static void SetBool(this ref DynamicProperty64 p, bool v) => SetRaw(ref p, new ValueUnion64 { asBool = v }.raw);

        public static double GetDouble(this in DynamicProperty64 p) => p.U().asDouble;
        public static void SetDouble(this ref DynamicProperty64 p, double v) => SetRaw(ref p, new ValueUnion64 { asDouble = v }.raw);

        public static long GetLong(this in DynamicProperty64 p) => p.U().asLong;
        public static void SetLong(this ref DynamicProperty64 p, long v) => SetRaw(ref p, new ValueUnion64 { asLong = v }.raw);

        // DateTime/TimeSpan as ticks if you want:
        public static DateTime GetDateTimeTicks(this in DynamicProperty64 p) => new(p.RawValue, DateTimeKind.Utc);
        public static void SetDateTimeTicks(this ref DynamicProperty64 p, DateTime value) => SetRaw(ref p, value.Ticks);

        public static TimeSpan GetTimeSpanTicks(this in DynamicProperty64 p) => new(p.RawValue);
        public static void SetTimeSpanTicks(this ref DynamicProperty64 p, TimeSpan value) => SetRaw(ref p, value.Ticks);

        // Enums (32-bit mask by default)
        public static TEnum GetEnum<TEnum>(this in DynamicProperty64 p) where TEnum : struct, Enum
            => (TEnum)Enum.ToObject(typeof(TEnum), p.U().asInt);
        public static void SetEnum<TEnum>(this ref DynamicProperty64 p, TEnum value) where TEnum : struct, Enum
            => SetInt(ref p, Convert.ToInt32(value));
        public static bool HasFlag<TEnum>(this in DynamicProperty64 p, TEnum flag) where TEnum : struct, Enum
            => (p.U().asInt & Convert.ToInt32(flag)) != 0;
        public static void AddFlag<TEnum>(this ref DynamicProperty64 p, TEnum flag) where TEnum : struct, Enum
            => SetInt(ref p, p.U().asInt | Convert.ToInt32(flag));
        public static void RemoveFlag<TEnum>(this ref DynamicProperty64 p, TEnum flag) where TEnum : struct, Enum
            => SetInt(ref p, p.U().asInt & ~Convert.ToInt32(flag));
        public static void ClearFlags(this ref DynamicProperty64 p) => SetInt(ref p, 0);
    }
}
