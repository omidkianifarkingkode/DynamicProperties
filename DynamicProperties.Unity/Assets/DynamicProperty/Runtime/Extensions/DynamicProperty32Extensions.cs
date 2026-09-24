using System;

namespace DynamicProperty
{
    public static class DynamicProperty32Extensions
    {
        private static ValueUnion32 U(this in DynamicProperty32 property)
        {
            return new ValueUnion32
            {
                raw = property.RawValue
            };
        }

        private static void SetRaw(ref DynamicProperty32 property, int raw)
        {
            property.RawValue = raw;
        }

        public static int GetInt(this in DynamicProperty32 property)
        {
            return property.U().asInt;
        }

        public static void SetInt(this ref DynamicProperty32 property, int value)
        {
            SetRaw(ref property, new ValueUnion32 { asInt = value }.raw);
        }

        public static float GetFloat(this in DynamicProperty32 property)
        {
            return property.U().asFloat;
        }

        public static void SetFloat(this ref DynamicProperty32 property, float value)
        {
            SetRaw(ref property, new ValueUnion32 { asFloat = value }.raw);
        }

        public static bool GetBool(this in DynamicProperty32 property)
        {
            return property.U().asBool;
        }

        public static void SetBool(this ref DynamicProperty32 property, bool value)
        {
            SetRaw(ref property, value ? 1 : 0);
        }

        public static TEnum GetEnum<TEnum>(this in DynamicProperty32 property)
            where TEnum : struct, Enum
        {
            return EnumBitUtility.FromRaw32<TEnum>(property.RawValue);
        }

        public static void SetEnum<TEnum>(this ref DynamicProperty32 property, TEnum value)
            where TEnum : struct, Enum
        {
            SetRaw(ref property, EnumBitUtility.ToRaw32(value));
        }

        public static bool HasFlag<TEnum>(this in DynamicProperty32 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            return EnumBitUtility.HasFlag(value, flag);
        }

        public static void AddFlag<TEnum>(this ref DynamicProperty32 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            property.SetEnum(EnumBitUtility.AddFlag(value, flag));
        }

        public static void RemoveFlag<TEnum>(this ref DynamicProperty32 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            property.SetEnum(EnumBitUtility.RemoveFlag(value, flag));
        }

        public static void ClearFlags(this ref DynamicProperty32 property)
        {
            SetRaw(ref property, 0);
        }
    }
}