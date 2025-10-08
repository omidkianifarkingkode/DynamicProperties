using System;

namespace DynamicProperty.Editor
{
    public sealed class PropertyMetadata
    {
        public PropertyValueType Type;
        public string DisplayName;
        public float? Min;
        public float? Max;
        public float? Step;
        public Type EnumType;
    }
}
