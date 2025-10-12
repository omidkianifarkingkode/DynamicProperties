using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ColorGroupAttribute : GroupAttribute
    {
        public ColorGroupAttribute(string name) : base(name) { }
    }

}
