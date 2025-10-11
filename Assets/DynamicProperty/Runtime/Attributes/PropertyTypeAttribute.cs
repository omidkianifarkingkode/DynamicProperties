using System;

namespace DynamicProperty
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public PropertyValueType Type { get; }
        public PropertyBitness Storage { get; }

        public PropertyTypeAttribute(PropertyValueType type, PropertyBitness storage = PropertyBitness.Both)
        {
            Type = type;
            Storage = storage;
        }
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

    [AttributeUsage(AttributeTargets.Field)]
    public class GroupAttribute : Attribute
    {
        public string Name { get; }
        public GroupAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Vector2GroupAttribute : GroupAttribute
    {
        public Vector2GroupAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Vector3GroupAttribute : GroupAttribute
    {
        public Vector3GroupAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ColorGroupAttribute : GroupAttribute
    {
        public ColorGroupAttribute(string name) : base(name) { }
    }

}
