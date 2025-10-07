using System;
using System.Collections.Generic;
using System.Reflection;

public static class PropertyMetadataCache
{
    public class Metadata
    {
        public PropertyType Type;
        public string DisplayName;
        public float? Min;
        public float? Max;
        public float? Step;
        public Type EnumType;
    }

    private static readonly Dictionary<PropertyKey, Metadata> cache = new();

    public static Metadata Get(PropertyKey key)
    {
        if (cache.TryGetValue(key, out var meta))
            return meta;

        var field = typeof(PropertyKey).GetField(key.ToString());
        var m = new Metadata();

        if (field.GetCustomAttribute<PropertyTypeAttribute>() is { } typeAttr)
            m.Type = typeAttr.Type;

        if (field.GetCustomAttribute<DisplayNameAttribute>() is { } nameAttr)
            m.DisplayName = nameAttr.DisplayName;

        if (field.GetCustomAttribute<MinMaxAttribute>() is { } minmax)
        {
            m.Min = minmax.Min;
            m.Max = minmax.Max;
        }

        if (field.GetCustomAttribute<StepAttribute>() is { } st)
            m.Step = st.Step;

        if (field.GetCustomAttribute<PropertyEnumAttribute>() is { } enumAttr)
            m.EnumType = enumAttr.EnumType;

        cache[key] = m;
        return m;
    }
}
