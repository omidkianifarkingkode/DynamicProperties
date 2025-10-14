using System;

namespace DynamicProperty.Editor
{
    public enum PropertyGroupKind { None, Vector2, Vector3, Vector4, Color }

    public sealed class PropertyMetadata
    {
        public PropertyValueType Type;
        public string DisplayName;
        public float? Min;
        public float? Max;
        public float? Step;
        public Type EnumType;
        public string GroupName;
        public object DefaultValue;
        public bool HiddenInEditor;
        public PropertyGroupKind GroupKind;
    }
}
