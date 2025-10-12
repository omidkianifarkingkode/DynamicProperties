using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Vector3GroupAttribute : GroupAttribute
    {
        public Vector3GroupAttribute(string name) : base(name) { }
    }

}
