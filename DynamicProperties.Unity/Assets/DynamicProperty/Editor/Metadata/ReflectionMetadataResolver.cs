using DynamicProperty.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DynamicProperty.Editor
{
    public sealed class ReflectionMetadataResolver : IPropertyMetadataResolver
    {
        private readonly Type _schemaType;
        private readonly Dictionary<int, PropertyMetadata> _byLogicalId = new();
        private readonly List<PropertyMetadata> _properties = new();

        public ReflectionMetadataResolver(Type schemaType)
        {
            if (schemaType == null)
                throw new ArgumentNullException(nameof(schemaType));

            if (!schemaType.IsInterface ||
                !typeof(IPropertySchema).IsAssignableFrom(schemaType))
            {
                throw new ArgumentException(
                    "Property schema type must be an interface implementing IPropertySchema.",
                    nameof(schemaType));
            }

            _schemaType = schemaType;

            Build();
        }

        public Type SchemaType => _schemaType;

        public IReadOnlyList<PropertyMetadata> Properties => _properties;

        public PropertyMetadata GetByLogicalId(int logicalId)
        {
            return _byLogicalId.TryGetValue(logicalId, out var metadata)
                ? metadata
                : null;
        }

        public PropertyMetadata GetByStorageId(int storageId)
        {
            if (!PropertyStorageId.TryDecode(storageId, out int logicalId, out _))
                return null;

            return GetByLogicalId(logicalId);
        }

        public string GetNameByStorageId(int storageId)
        {
            return GetByStorageId(storageId)?.Name;
        }

        private void Build()
        {
            foreach (var property in _schemaType.GetProperties())
            {
                var propertyAttr =
                    property.GetCustomAttribute<DynamicProperty.DataAnnotations.PropertyAttribute>();

                if (propertyAttr == null)
                    continue;

                var metadata = new PropertyMetadata
                {
                    LogicalId = propertyAttr.Id,
                    Name = property.Name,
                    DisplayName = GetDisplayName(property),
                    AggregateKind = GetAggregateKind(property.PropertyType)
                };

                ApplyTypeMetadata(metadata, property.PropertyType);
                ApplyInitialValue(metadata, property);
                ApplyCommonMetadata(metadata, property);

                if (_byLogicalId.ContainsKey(metadata.LogicalId))
                    continue;

                _byLogicalId.Add(metadata.LogicalId, metadata);
                _properties.Add(metadata);
            }

            _properties.Sort(CompareMetadata);
        }

        private static int CompareMetadata(PropertyMetadata left, PropertyMetadata right)
        {
            string leftCategory = NormalizeCategory(left.Category);
            string rightCategory = NormalizeCategory(right.Category);

            int categoryComparison =
                string.Compare(leftCategory, rightCategory, StringComparison.OrdinalIgnoreCase);

            if (categoryComparison != 0)
                return categoryComparison;

            int leftOrder = left.Order ?? int.MaxValue;
            int rightOrder = right.Order ?? int.MaxValue;

            int orderComparison =
                leftOrder.CompareTo(rightOrder);

            if (orderComparison != 0)
                return orderComparison;

            return left.LogicalId.CompareTo(right.LogicalId);
        }

        private static void ApplyTypeMetadata(
            PropertyMetadata meta,
            Type propertyType)
        {
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
            else if (propertyType.IsEnum)
            {
                meta.Type = PropertyValueType.Enum;
                meta.EnumType = propertyType;
            }
            else if (propertyType == typeof(Vector2) ||
                     propertyType == typeof(Vector3) ||
                     propertyType == typeof(Vector4) ||
                     propertyType == typeof(Color))
            {
                meta.Type = PropertyValueType.Float;
            }
            else
            {
                throw new NotSupportedException(
                    $"Property schema type '{propertyType.FullName}' is unsupported.");
            }
        }

        private static void ApplyInitialValue(
            PropertyMetadata meta,
            PropertyInfo property)
        {
            if (property.GetCustomAttribute<InitialValueAttribute>() is { } initialValue)
            {
                meta.HasInitialValue = true;
                meta.InitialValue = initialValue.Value;
            }
        }

        private static void ApplyCommonMetadata(
            PropertyMetadata meta,
            MemberInfo member)
        {
            if (member.GetCustomAttribute(typeof(DisplayNameAttribute)) is DisplayNameAttribute dn)
                meta.DisplayName = dn.DisplayName;

            if (member.GetCustomAttribute(typeof(MinMaxAttribute)) is MinMaxAttribute mm)
            {
                meta.Min = mm.Min;
                meta.Max = mm.Max;
            }

            if (member.GetCustomAttribute(typeof(StepAttribute)) is StepAttribute st)
                meta.Step = st.Step;

            if (member.GetCustomAttribute<PropertyEditorIgnoreAttribute>() != null)
                meta.HiddenInEditor = true;

            if (member.GetCustomAttribute<PropertyTooltipAttribute>() is { } tooltip)
                meta.Tooltip = tooltip.Text;

            if (member.GetCustomAttribute<PropertyCategoryAttribute>() is { } category)
                meta.Category = category.Name;

            if (member.GetCustomAttribute<PropertyOrderAttribute>() is { } order)
                meta.Order = order.Order;
        }

        private static string GetDisplayName(PropertyInfo property)
        {
            if (property.GetCustomAttribute(typeof(DisplayNameAttribute)) is DisplayNameAttribute dn)
                return dn.DisplayName;

            return NicifyName(property.Name);
        }

        private static string NicifyName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var chars =
                new List<char>(name.Length + 4);

            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];

                if (i > 0 &&
                    char.IsUpper(ch) &&
                    !char.IsWhiteSpace(name[i - 1]))
                {
                    chars.Add(' ');
                }

                chars.Add(ch);
            }

            return new string(chars.ToArray());
        }

        private static PropertyAggregateKind GetAggregateKind(Type propertyType)
        {
            if (propertyType == typeof(Vector2))
                return PropertyAggregateKind.Vector2;

            if (propertyType == typeof(Vector3))
                return PropertyAggregateKind.Vector3;

            if (propertyType == typeof(Vector4))
                return PropertyAggregateKind.Vector4;

            if (propertyType == typeof(Color))
                return PropertyAggregateKind.Color;

            return PropertyAggregateKind.None;
        }

        private static string NormalizeCategory(string category)
        {
            return string.IsNullOrWhiteSpace(category)
                ? "Default"
                : category.Trim();
        }
    }
}
