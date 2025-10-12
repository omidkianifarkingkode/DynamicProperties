using System;

namespace DynamicProperty
{
    public static class DynamicProperty32Extensions
    {
        private static ValueUnion32 U(this in DynamicProperty32 p) => new ValueUnion32 { raw = p.RawValue };
        private static void SetRaw(ref DynamicProperty32 p, int raw) => p.RawValue = raw;


        public static int GetInt(this in DynamicProperty32 p) => p.U().asInt;
        public static void SetInt(this ref DynamicProperty32 p, int v) => SetRaw(ref p, new ValueUnion32 { asInt = v }.raw);

        public static float GetFloat(this in DynamicProperty32 p) => p.U().asFloat;
        public static void SetFloat(this ref DynamicProperty32 p, float v) => SetRaw(ref p, new ValueUnion32 { asFloat = v }.raw);

        public static bool GetBool(this in DynamicProperty32 p) => p.U().asBool;
        public static void SetBool(this ref DynamicProperty32 p, bool v) => SetRaw(ref p, new ValueUnion32 { asBool = v }.raw);

        public static TEnum GetEnum<TEnum>(this in DynamicProperty32 p) where TEnum : struct, Enum
            => (TEnum)Enum.ToObject(typeof(TEnum), p.U().asInt);
        public static void SetEnum<TEnum>(this ref DynamicProperty32 p, TEnum value) where TEnum : struct, Enum
            => SetInt(ref p, Convert.ToInt32(value));

        public static bool HasFlag<TEnum>(this in DynamicProperty32 p, TEnum flag) where TEnum : struct, Enum
            => (p.U().asInt & Convert.ToInt32(flag)) != 0;
        public static void AddFlag<TEnum>(this ref DynamicProperty32 p, TEnum flag) where TEnum : struct, Enum
            => SetInt(ref p, p.U().asInt | Convert.ToInt32(flag));
        public static void RemoveFlag<TEnum>(this ref DynamicProperty32 p, TEnum flag) where TEnum : struct, Enum
            => SetInt(ref p, p.U().asInt & ~Convert.ToInt32(flag));
        public static void ClearFlags(this ref DynamicProperty32 p) => SetInt(ref p, 0);
    }
}
