using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StepAttribute : Attribute
    {
        public float Step { get; }
        public StepAttribute(float step) => Step = step;
    }

}
