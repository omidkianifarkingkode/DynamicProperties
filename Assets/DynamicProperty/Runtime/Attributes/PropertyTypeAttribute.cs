using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public Type Type { get; }
        public string GroupName { get; }
        public object DefaultValue { get; }

        // Constructor to specify only the type
        public PropertyTypeAttribute(Type type)
        {
            Type = type;
        }

        // Constructor to specify the type and group name (for things like Vector3, Color, etc.)
        public PropertyTypeAttribute(Type type, string groupName)
        {
            Type = type;
            GroupName = groupName;
        }

        // Constructor to specify the type and the default value
        public PropertyTypeAttribute(Type type, object defaultValue)
        {
            Type = type;
            DefaultValue = defaultValue;
        }

        // Constructor to specify the type, default value, and group name
        public PropertyTypeAttribute(Type type, object defaultValue, string groupName)
        {
            Type = type;
            DefaultValue = defaultValue;
            GroupName = groupName;
        }
    }
}
