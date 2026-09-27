using System;
using DynamicProperty.DataAnnotations;

namespace DynamicProperty.Editor
{
    public enum PropertyGroupKind { None, Vector2, Vector3, Vector4, Color }

    public sealed class PropertyMetadata
    {
        public PropertyValueType Type;

        public string DisplayName;
        public string Tooltip;
        public string Category;
        public int? Order;

        public float? Min;
        public float? Max;
        public float? Step;

        public Type EnumType;

        public bool HasInitialValue;
        public object InitialValue;

        public bool HiddenInEditor;

        public string GroupName;
        public PropertyGroupKind GroupKind;
        public PropertyComponent? GroupComponent;
    }
}
