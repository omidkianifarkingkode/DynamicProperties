using System;

public enum PropertyType { Int, Float, Bool, Double, Long, DateTime, TimeSpan, Enum }

[AttributeUsage(AttributeTargets.Field)]
public class PropertyTypeAttribute : Attribute
{
    public PropertyType Type { get; }
    public PropertyTypeAttribute(PropertyType type) => Type = type;
}

[AttributeUsage(AttributeTargets.Field)]
public class DisplayNameAttribute : Attribute
{
    public string DisplayName { get; }
    public DisplayNameAttribute(string displayName) => DisplayName = displayName;
}

[AttributeUsage(AttributeTargets.Field)]
public class MinMaxAttribute : Attribute
{
    public float Min { get; }
    public float Max { get; }
    public MinMaxAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class StepAttribute : Attribute
{
    public float Step { get; }
    public StepAttribute(float step) => Step = step;
}

[AttributeUsage(AttributeTargets.Field)]
public class PropertyEnumAttribute : Attribute
{
    public Type EnumType { get; }
    public PropertyEnumAttribute(Type enumType)
    {
        EnumType = enumType;
    }
}

