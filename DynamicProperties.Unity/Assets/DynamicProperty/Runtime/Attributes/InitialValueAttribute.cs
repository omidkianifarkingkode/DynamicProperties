using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InitialValueAttribute : Attribute
    {
        public object Value { get; }

        public InitialValueAttribute(object value)
        {
            Value = value;
        }
    }
}
