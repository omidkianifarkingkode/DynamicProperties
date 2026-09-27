using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyTooltipAttribute : Attribute
    {
        public string Text { get; }

        public PropertyTooltipAttribute(string text)
        {
            Text = text;
        }
    }
}
