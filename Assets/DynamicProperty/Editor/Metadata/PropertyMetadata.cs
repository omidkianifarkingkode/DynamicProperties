using System;

namespace DynamicProperty.Editor
{
    public enum PropertyGroupKind { None, Vector2, Vector3, Color }

    public sealed class PropertyMetadata
    {
        public PropertyValueType Type;
        public PropertyBitness Bitness;
        public string DisplayName;
        public float? Min;
        public float? Max;
        public float? Step;
        public Type EnumType;
        public string GroupName;

        public PropertyGroupKind GroupKind   // helper derived prop
        {
            get
            {
                if (string.IsNullOrEmpty(GroupName)) return PropertyGroupKind.None;
                var g = GroupName.ToLowerInvariant();
                if (g == "color") return PropertyGroupKind.Color;
                if (g == "vector2" || g == "vec2") return PropertyGroupKind.Vector2;
                if (g == "vector3" || g == "vec3") return PropertyGroupKind.Vector3;
                return PropertyGroupKind.None;
            }
        }
    }
}
