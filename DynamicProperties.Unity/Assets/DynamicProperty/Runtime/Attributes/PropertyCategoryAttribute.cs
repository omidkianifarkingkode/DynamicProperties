using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyCategoryAttribute : Attribute
    {
        public string Name { get; }

        public PropertyCategoryAttribute(string name)
        {
            Name = name;
        }
    }
}
