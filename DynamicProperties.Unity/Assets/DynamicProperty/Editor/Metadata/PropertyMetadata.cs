using System;

namespace DynamicProperty.Editor
{
    public enum PropertyAggregateKind { None, Vector2, Vector3, Vector4, Color }

    public sealed class PropertyMetadata
    {
        public int LogicalId;
        public string Name;
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

        public PropertyAggregateKind AggregateKind;
    }
}
