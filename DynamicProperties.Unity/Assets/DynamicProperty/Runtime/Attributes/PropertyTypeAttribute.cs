using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public Type Type { get; }

        public bool HasInitialValue { get; }

        public object InitialValue { get; }

        public PropertyTypeAttribute(Type type)
        {
            Type = type;
            HasInitialValue = false;
            InitialValue = null;
        }

        public PropertyTypeAttribute(Type type, object initialValue)
        {
            Type = type;
            HasInitialValue = true;
            InitialValue = initialValue;
        }
    }
}