using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Vector2GroupAttribute : GroupAttribute
    {
        public Vector2GroupAttribute(string name) : base(name) { }
    }

}
