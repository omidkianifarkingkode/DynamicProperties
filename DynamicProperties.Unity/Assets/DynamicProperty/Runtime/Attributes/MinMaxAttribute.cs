using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MinMaxAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }
        public MinMaxAttribute(float min, float max) { Min = min; Max = max; }
    }

}
