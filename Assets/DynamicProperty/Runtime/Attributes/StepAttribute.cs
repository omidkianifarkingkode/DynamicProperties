using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StepAttribute : Attribute
    {
        public float Step { get; }
        public StepAttribute(float step) => Step = step;
    }

}
