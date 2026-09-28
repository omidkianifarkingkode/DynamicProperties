using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyAttribute : Attribute
    {
        public int Id { get; }

        public PropertyAttribute(int id)
        {
            Id = id;
        }
    }
}
