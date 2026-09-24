using System;

namespace DynamicProperty
{
    internal static class EnumBitUtility
    {
        public static bool Uses64BitStorage<TEnum>()
            where TEnum : struct, Enum
        {
            return Uses64BitStorage(typeof(TEnum));
        }

        public static bool Uses64BitStorage(Type enumType)
        {
            ValidateEnumType(enumType);

            var underlying = Enum.GetUnderlyingType(enumType);

            return underlying == typeof(long) ||
                   underlying == typeof(ulong);
        }

        public static int ToRaw32<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return ToRaw32(typeof(TEnum), (Enum)(object)value);
        }

        public static int ToRaw32(Type enumType, Enum value)
        {
            if (Uses64BitStorage(enumType))
            {
                throw new InvalidOperationException(
                    $"Enum '{enumType.FullName}' requires 64-bit storage.");
            }

            ulong bits = ToUInt64Bits(enumType, value);

            return unchecked((int)(uint)bits);
        }

        public static long ToRaw64<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return ToRaw64(typeof(TEnum), (Enum)(object)value);
        }

        public static long ToRaw64(Type enumType, Enum value)
        {
            if (!Uses64BitStorage(enumType))
            {
                throw new InvalidOperationException(
                    $"Enum '{enumType.FullName}' uses 32-bit storage.");
            }

            ulong bits = ToUInt64Bits(enumType, value);

            return unchecked((long)bits);
        }

        public static TEnum FromRaw32<TEnum>(int raw)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            if (Uses64BitStorage(type))
            {
                throw new InvalidOperationException(
                    $"Enum '{type.FullName}' requires 64-bit storage.");
            }

            return (TEnum)(object)FromUInt64Bits(type, unchecked((uint)raw));
        }

        public static Enum FromRaw32(Type enumType, int raw)
        {
            if (Uses64BitStorage(enumType))
            {
                throw new InvalidOperationException(
                    $"Enum '{enumType.FullName}' requires 64-bit storage.");
            }

            return FromUInt64Bits(enumType, unchecked((uint)raw));
        }

        public static TEnum FromRaw64<TEnum>(long raw)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            if (!Uses64BitStorage(type))
            {
                throw new InvalidOperationException(
                    $"Enum '{type.FullName}' uses 32-bit storage.");
            }

            return (TEnum)(object)FromUInt64Bits(type, unchecked((ulong)raw));
        }

        public static Enum FromRaw64(Type enumType, long raw)
        {
            if (!Uses64BitStorage(enumType))
            {
                throw new InvalidOperationException(
                    $"Enum '{enumType.FullName}' uses 32-bit storage.");
            }

            return FromUInt64Bits(enumType, unchecked((ulong)raw));
        }

        public static bool HasFlag<TEnum>(TEnum value, TEnum flag)
            where TEnum : struct, Enum
        {
            ulong valueBits = ToUInt64Bits(typeof(TEnum), value);

            ulong flagBits = ToUInt64Bits(typeof(TEnum), flag);

            if (flagBits == 0)
                return valueBits == 0;

            return (valueBits & flagBits) == flagBits;
        }

        public static TEnum AddFlag<TEnum>(TEnum value, TEnum flag)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            ulong result =
                ToUInt64Bits(type, value) |
                ToUInt64Bits(type, flag);

            result &= GetStorageMask(type);

            return (TEnum)(object) FromUInt64Bits(type, result);
        }

        public static TEnum RemoveFlag<TEnum>(TEnum value, TEnum flag)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            ulong result =
                ToUInt64Bits(type, value) &
                ~ToUInt64Bits(type, flag);

            result &= GetStorageMask(type);

            return (TEnum)(object) FromUInt64Bits(type, result);
        }

        public static ulong ToUInt64Bits(Type enumType, object value)
        {
            ValidateEnumType(enumType);

            var underlying = Enum.GetUnderlyingType(enumType);

            if (underlying == typeof(sbyte))
            {
                sbyte v = Convert.ToSByte(value);
                return unchecked((byte)v);
            }

            if (underlying == typeof(byte))
                return Convert.ToByte(value);

            if (underlying == typeof(short))
            {
                short v = Convert.ToInt16(value);
                return unchecked((ushort)v);
            }

            if (underlying == typeof(ushort))
                return Convert.ToUInt16(value);

            if (underlying == typeof(int))
            {
                int v = Convert.ToInt32(value);
                return unchecked((uint)v);
            }

            if (underlying == typeof(uint))
                return Convert.ToUInt32(value);

            if (underlying == typeof(long))
            {
                long v = Convert.ToInt64(value);
                return unchecked((ulong)v);
            }

            if (underlying == typeof(ulong))
                return Convert.ToUInt64(value);

            throw new NotSupportedException($"Unsupported enum underlying type '{underlying.FullName}'.");
        }

        public static Enum FromUInt64Bits(Type enumType, ulong bits)
        {
            ValidateEnumType(enumType);

            bits &= GetStorageMask(enumType);

            var underlying = Enum.GetUnderlyingType(enumType);

            object value;

            if (underlying == typeof(sbyte))
                value = unchecked((sbyte)(byte)bits);

            else if (underlying == typeof(byte))
                value = unchecked((byte)bits);

            else if (underlying == typeof(short))
                value = unchecked((short)(ushort)bits);

            else if (underlying == typeof(ushort))
                value = unchecked((ushort)bits);

            else if (underlying == typeof(int))
                value = unchecked((int)(uint)bits);

            else if (underlying == typeof(uint))
                value = unchecked((uint)bits);

            else if (underlying == typeof(long))
                value = unchecked((long)bits);

            else if (underlying == typeof(ulong))
                value = bits;

            else
                throw new NotSupportedException($"Unsupported enum underlying type '{underlying.FullName}'.");

            return (Enum)Enum.ToObject(enumType, value);
        }

        public static ulong GetStorageMask(Type enumType)
        {
            ValidateEnumType(enumType);

            var underlying = Enum.GetUnderlyingType(enumType);

            if (underlying == typeof(sbyte) ||
                underlying == typeof(byte))
            {
                return 0xFFUL;
            }

            if (underlying == typeof(short) ||
                underlying == typeof(ushort))
            {
                return 0xFFFFUL;
            }

            if (underlying == typeof(int) ||
                underlying == typeof(uint))
            {
                return 0xFFFFFFFFUL;
            }

            return ulong.MaxValue;
        }

        public static ulong GetDefinedBitsMask(Type enumType)
        {
            ValidateEnumType(enumType);

            ulong mask = 0;

            foreach (var value in Enum.GetValues(enumType))
            {
                mask |= ToUInt64Bits(enumType, value);
            }

            return mask & GetStorageMask(enumType);
        }

        private static void ValidateEnumType(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                    $"Type '{enumType.FullName}' is not an enum.",
                    nameof(enumType));
            }
        }
    }
}