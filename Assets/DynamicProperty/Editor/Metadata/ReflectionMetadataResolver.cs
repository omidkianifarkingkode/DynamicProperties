using DynamicProperty.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DynamicProperty.Editor
{
    public sealed class ReflectionMetadataResolver : IPropertyMetadataResolver
    {
        readonly Type _enumType;
        readonly Dictionary<int, PropertyMetadata> _cache = new();

        public ReflectionMetadataResolver(Type enumType)
        {
            _enumType = enumType;
        }

        public Type BoundEnumType => _enumType;

        public PropertyMetadata Get(int id)
        {
            if (_enumType == null) return null;
            if (_cache.TryGetValue(id, out var m)) return m;

            string name = Enum.GetName(_enumType, id);
            if (name == null) return null;

            var field = _enumType.GetField(name);
            var meta = new PropertyMetadata();

            // Get PropertyType attribute applied to the enum value
            var propertyTypeAttr = field.GetCustomAttribute<PropertyTypeAttribute>();
            if (propertyTypeAttr != null)
            {
                var propertyType = propertyTypeAttr.Type;

                // Handle basic types and enums
                if (propertyType == typeof(float))
                {
                    meta.Type = PropertyValueType.Float;
                }
                else if (propertyType == typeof(int))
                {
                    meta.Type = PropertyValueType.Int;
                }
                else if (propertyType == typeof(bool))
                {
                    meta.Type = PropertyValueType.Bool;
                }
                else if (propertyType == typeof(long))
                {
                    meta.Type = PropertyValueType.Long;
                }
                else if (propertyType == typeof(double))
                {
                    meta.Type = PropertyValueType.Double;
                }
                else if (propertyType == typeof(DateTime))
                {
                    meta.Type = PropertyValueType.DateTime;
                }
                else if (propertyType == typeof(TimeSpan))
                {
                    meta.Type = PropertyValueType.TimeSpan;
                }
                else if (propertyType.IsEnum) // Detect if it's an Enum
                {
                    meta.Type = PropertyValueType.Enum;
                    meta.EnumType = propertyType;
                }
                else if (propertyType == typeof(Vector4)) // Special case for Vector3 grouping
                {
                    meta.Type = PropertyValueType.Float;
                    meta.GroupKind = PropertyGroupKind.Vector4;
                }
                else if (propertyType == typeof(Vector3)) // Special case for Vector3 grouping
                {
                    meta.Type = PropertyValueType.Float;
                    meta.GroupKind = PropertyGroupKind.Vector3;
                }
                else if (propertyType == typeof(Vector2)) // Special case for Vector3 grouping
                {
                    meta.Type = PropertyValueType.Float;
                    meta.GroupKind = PropertyGroupKind.Vector2;
                }
                else if (propertyType == typeof(Color)) // Special case for Color grouping
                {
                    meta.Type = PropertyValueType.Float;
                    meta.GroupKind = PropertyGroupKind.Color;
                }

                if (propertyTypeAttr.DefaultValue != null)
                {
                    meta.DefaultValue = propertyTypeAttr.DefaultValue;
                }
            }

            // Detect other attributes like DisplayName, MinMax, etc.
            if (field.GetCustomAttribute(typeof(DisplayNameAttribute)) is DisplayNameAttribute dn)
                meta.DisplayName = dn.DisplayName;

            if (field.GetCustomAttribute(typeof(MinMaxAttribute)) is MinMaxAttribute mm)
            {
                meta.Min = mm.Min;
                meta.Max = mm.Max;
            }

            if (field.GetCustomAttribute(typeof(StepAttribute)) is StepAttribute st)
                meta.Step = st.Step;

            if (field.GetCustomAttribute(typeof(GroupAttribute)) is GroupAttribute grp)
                meta.GroupName = grp.Name;

            _cache[id] = meta;
            return meta;
        }

        public string[] GetAllNames() => _enumType == null ? Array.Empty<string>() : Enum.GetNames(_enumType);

        public int[] GetAllValues()
        {
            if (_enumType == null) return Array.Empty<int>();
            var values = (Array)Enum.GetValues(_enumType);
            var res = new int[values.Length];
            for (int i = 0; i < res.Length; i++) res[i] = Convert.ToInt32(values.GetValue(i));
            return res;
        }
    }
}
