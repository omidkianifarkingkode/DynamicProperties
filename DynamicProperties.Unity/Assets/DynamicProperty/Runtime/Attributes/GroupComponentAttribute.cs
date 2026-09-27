using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class GroupComponentAttribute : Attribute
    {
        public PropertyComponent Component { get; }

        public GroupComponentAttribute(PropertyComponent component)
        {
            Component = component;
        }
    }
}