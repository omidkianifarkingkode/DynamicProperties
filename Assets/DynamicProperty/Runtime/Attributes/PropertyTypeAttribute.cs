using System;

namespace DynamicProperty
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public PropertyValueType Type { get; }
        public PropertyTypeAttribute(PropertyValueType type) => Type = type;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public DisplayNameAttribute(string displayName) => DisplayName = displayName;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }
        public MinMaxAttribute(float min, float max) { Min = min; Max = max; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StepAttribute : Attribute
    {
        public float Step { get; }
        public StepAttribute(float step) => Step = step;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyEnumAttribute : Attribute
    {
        public Type EnumType { get; }
        public PropertyEnumAttribute(Type enumType) => EnumType = enumType;
    }
}
