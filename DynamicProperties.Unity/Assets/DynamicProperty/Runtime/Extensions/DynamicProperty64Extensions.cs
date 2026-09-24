using System;

namespace DynamicProperty
{
    public static class DynamicProperty64Extensions
    {
        private static ValueUnion64 U(this in DynamicProperty64 property)
        {
            return new ValueUnion64
            {
                raw = property.RawValue
            };
        }

        private static void SetRaw(ref DynamicProperty64 property, long raw)
        {
            property.RawValue = raw;
        }

        public static int GetInt(this in DynamicProperty64 property)
        {
            return property.U().asInt;
        }

        public static void SetInt(this ref DynamicProperty64 property, int value)
        {
            SetRaw(ref property, new ValueUnion64 { asInt = value }.raw);
        }

        public static float GetFloat(this in DynamicProperty64 property)
        {
            return property.U().asFloat;
        }

        public static void SetFloat(this ref DynamicProperty64 property, float value)
        {
            SetRaw(ref property, new ValueUnion64 { asFloat = value }.raw);
        }

        public static bool GetBool(this in DynamicProperty64 property)
        {
            return property.U().asBool;
        }

        public static void SetBool(this ref DynamicProperty64 property, bool value)
        {
            SetRaw(ref property, value ? 1L : 0L);
        }

        public static double GetDouble(this in DynamicProperty64 property)
        {
            return property.U().asDouble;
        }

        public static void SetDouble(this ref DynamicProperty64 property, double value)
        {
            SetRaw(ref property, new ValueUnion64 { asDouble = value }.raw);
        }

        public static long GetLong(this in DynamicProperty64 property)
        {
            return property.RawValue;
        }

        public static void SetLong(this ref DynamicProperty64 property, long value)
        {
            SetRaw(ref property, value);
        }

        public static DateTime GetUtcDateTime(this in DynamicProperty64 property)
        {
            return new DateTime(property.RawValue, DateTimeKind.Utc);
        }

        public static bool TryGetUtcDateTime(this in DynamicProperty64 property, out DateTime value)
        {
            long ticks = property.RawValue;

            if (ticks < DateTime.MinValue.Ticks ||
                ticks > DateTime.MaxValue.Ticks)
            {
                value = default;
                return false;
            }

            value = new DateTime(ticks, DateTimeKind.Utc);

            return true;
        }

        public static void SetUtcDateTime(this ref DynamicProperty64 property, DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "DateTime value must use DateTimeKind.Utc.",
                    nameof(value));
            }

            property.RawValue = value.Ticks;
        }

        public static TimeSpan GetTimeSpanTicks(this in DynamicProperty64 property)
        {
            return new TimeSpan(property.RawValue);
        }

        public static void SetTimeSpanTicks(this ref DynamicProperty64 property, TimeSpan value)
        {
            SetRaw(ref property, value.Ticks);
        }

        public static TEnum GetEnum<TEnum>(this in DynamicProperty64 property)
            where TEnum : struct, Enum
        {
            return EnumBitUtility.FromRaw64<TEnum>(property.RawValue);
        }

        public static void SetEnum<TEnum>(this ref DynamicProperty64 property, TEnum value)
            where TEnum : struct, Enum
        {
            SetRaw(ref property, EnumBitUtility.ToRaw64(value));
        }

        public static bool HasFlag<TEnum>(this in DynamicProperty64 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            return EnumBitUtility.HasFlag(value, flag);
        }

        public static void AddFlag<TEnum>(this ref DynamicProperty64 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            property.SetEnum(EnumBitUtility.AddFlag(value, flag));
        }

        public static void RemoveFlag<TEnum>(this ref DynamicProperty64 property, TEnum flag)
            where TEnum : struct, Enum
        {
            var value = property.GetEnum<TEnum>();

            property.SetEnum(EnumBitUtility.RemoveFlag(value, flag));
        }

        public static void ClearFlags(this ref DynamicProperty64 property)
        {
            SetRaw(ref property, 0L);
        }
    }
}