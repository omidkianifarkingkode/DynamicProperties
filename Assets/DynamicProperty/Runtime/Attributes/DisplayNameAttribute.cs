using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public DisplayNameAttribute(string displayName) => DisplayName = displayName;
    }

}
