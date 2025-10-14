using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyEnumAttribute : Attribute
    {
        public Type EnumType { get; }
        public PropertyEnumAttribute(Type enumType) => EnumType = enumType;
    }

}
