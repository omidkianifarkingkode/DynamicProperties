using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public Type Type { get; }
        public object DefaultValue { get; }

        // Constructor to specify only the type
        public PropertyTypeAttribute(Type type)
        {
            Type = type;
        }

        // Constructor to specify the type and the default value
        public PropertyTypeAttribute(Type type, object defaultValue)
        {
            Type = type;
            DefaultValue = defaultValue;
        }
    }
}
