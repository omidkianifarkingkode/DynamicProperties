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
        readonly Type _schemaType;
        readonly bool _isEnumSchema;
        readonly Dictionary<int, PropertyMetadata> _cache = new();
        readonly Dictionary<int, string> _names = new();

        int[] _allValues;
        string[] _allNames;
        bool _interfaceCacheBuilt;

        public ReflectionMetadataResolver(Type schemaType)
        {
            if (schemaType == null)
            {
                throw new ArgumentNullException(nameof(schemaType));
            }

            if (schemaType.IsEnum)
            {
                if (Enum.GetUnderlyingType(schemaType) != typeof(int))
                {
                    throw new ArgumentException(
                        "Property schema enum must use int as its underlying type.",
                        nameof(schemaType));
                }

                _isEnumSchema = true;
            }
            else if (schemaType.IsInterface &&
                     typeof(IPropertySchema).IsAssignableFrom(schemaType))
            {
                _isEnumSchema = false;
            }
            else
            {
                throw new ArgumentException(
                    "Property schema type must be an enum or an interface implementing IPropertySchema.",
                    nameof(schemaType));
            }

            _schemaType = schemaType;
        }

        public Type BoundEnumType => _schemaType;

        public PropertyMetadata Get(int id)
        {
            if (_isEnumSchema)
                return GetEnumMetadata(id);

            EnsureInterfaceCache();

            return _cache.TryGetValue(id, out var metadata)
                ? metadata
                : null;
        }

        public string GetName(int id)
        {
            if (_isEnumSchema)
                return Enum.GetName(_schemaType, id);

            EnsureInterfaceCache();

            return _names.TryGetValue(id, out var name)
                ? name
                : null;
        }

        public string[] GetAllNames()
        {
            if (_isEnumSchema)
                return Enum.GetNames(_schemaType);

            EnsureInterfaceCache();

            return _allNames;
        }

        public int[] GetAllValues()
        {
            if (_isEnumSchema)
            {
                var values = (Array)Enum.GetValues(_schemaType);

                var result = new int[values.Length];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = Convert.ToInt32(values.GetValue(i));
                }

                return result;
            }

            EnsureInterfaceCache();

            return _allValues;
        }

        PropertyMetadata GetEnumMetadata(int id)
        {
            if (_cache.TryGetValue(id, out var m)) return m;

            string name = Enum.GetName(_schemaType, id);
            if (name == null) return null;

            var field = _schemaType.GetField(name);
            var meta = new PropertyMetadata();

            var propertyTypeAttr = field.GetCustomAttribute<PropertyTypeAttribute>();

            if (propertyTypeAttr == null)
                return null;

            var propertyType = propertyTypeAttr.Type;

            ApplyTypeMetadata(meta, propertyType);

            if (propertyTypeAttr.HasInitialValue)
            {
                meta.HasInitialValue = true;
                meta.InitialValue = propertyTypeAttr.InitialValue;
            }

            ApplyCommonMetadata(meta, field);

            _cache[id] = meta;
            return meta;
        }

        void EnsureInterfaceCache()
        {
            if (_interfaceCacheBuilt)
                return;

            var orderedValues = new List<int>();
            var orderedNames = new List<string>();

            foreach (var property in _schemaType.GetProperties())
            {
                var propertyAttr =
                    property.GetCustomAttribute<DynamicProperty.DataAnnotations.PropertyAttribute>();

                if (propertyAttr == null)
                    continue;

                var ids =
                    propertyAttr.Ids?.ToArray() ??
                    Array.Empty<int>();

                var groupKind =
                    GetGroupKind(property.PropertyType);

                if (groupKind == PropertyGroupKind.None)
                {
                    if (ids.Length != 1)
                        continue;

                    var meta = new PropertyMetadata();

                    ApplyTypeMetadata(meta, property.PropertyType);
                    ApplyInitialValue(meta, property);
                    ApplyCommonMetadata(meta, property);

                    AddInterfaceMetadata(
                        ids[0],
                        property.Name,
                        meta,
                        orderedValues,
                        orderedNames);

                    continue;
                }

                string groupName =
                    GetLogicalDisplayName(property);

                var components =
                    GetComponents(groupKind);

                if (ids.Length != components.Length)
                    continue;

                for (int i = 0; i < ids.Length; i++)
                {
                    var meta = new PropertyMetadata
                    {
                        Type = PropertyValueType.Float,
                        GroupKind = groupKind,
                        GroupName = groupName,
                        GroupComponent = components[i]
                    };

                    ApplyCommonMetadata(meta, property);

                    AddInterfaceMetadata(
                        ids[i],
                        groupName + " " + components[i],
                        meta,
                        orderedValues,
                        orderedNames);
                }
            }

            _allValues = orderedValues.ToArray();
            _allNames = orderedNames.ToArray();
            _interfaceCacheBuilt = true;
        }

        static void AddInterfaceName(
            int id,
            string name,
            PropertyMetadata metadata,
            List<int> orderedValues,
            List<string> orderedNames)
        {
            orderedValues.Add(id);
            orderedNames.Add(name);
            metadata.DisplayName ??= name;
        }

        void AddInterfaceMetadata(
            int id,
            string name,
            PropertyMetadata metadata,
            List<int> orderedValues,
            List<string> orderedNames)
        {
            _cache[id] = metadata;
            _names[id] = name;

            AddInterfaceName(
                id,
                name,
                metadata,
                orderedValues,
                orderedNames);
        }

        static void ApplyTypeMetadata(
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
            else if (propertyType == typeof(Vector2))
            {
                meta.Type = PropertyValueType.Float;
                meta.GroupKind = PropertyGroupKind.Vector2;
            }
            else if (propertyType == typeof(Vector3))
            {
                meta.Type = PropertyValueType.Float;
                meta.GroupKind = PropertyGroupKind.Vector3;
            }
            else if (propertyType == typeof(Vector4))
            {
                meta.Type = PropertyValueType.Float;
                meta.GroupKind = PropertyGroupKind.Vector4;
            }
            else if (propertyType == typeof(Color))
            {
                meta.Type = PropertyValueType.Float;
                meta.GroupKind = PropertyGroupKind.Color;
            }
            else
            {
                throw new NotSupportedException(
                    $"Property schema type '{propertyType.FullName}' is unsupported.");
            }
        }

        static void ApplyInitialValue(
            PropertyMetadata meta,
            PropertyInfo property)
        {
            if (property.GetCustomAttribute<InitialValueAttribute>() is { } initialValue)
            {
                meta.HasInitialValue = true;
                meta.InitialValue = initialValue.Value;
            }
        }

        static void ApplyCommonMetadata(
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

            if (member.GetCustomAttribute(typeof(GroupAttribute)) is GroupAttribute grp)
                meta.GroupName = grp.Name;

            if (member.GetCustomAttribute<GroupComponentAttribute>() is { } component)
                meta.GroupComponent = component.Component;

            if (member.GetCustomAttribute<PropertyEditorIgnoreAttribute>() != null)
                meta.HiddenInEditor = true;

            if (member.GetCustomAttribute<PropertyTooltipAttribute>() is { } tooltip)
                meta.Tooltip = tooltip.Text;

            if (member.GetCustomAttribute<PropertyCategoryAttribute>() is { } category)
                meta.Category = category.Name;

            if (member.GetCustomAttribute<PropertyOrderAttribute>() is { } order)
                meta.Order = order.Order;
        }

        static string GetLogicalDisplayName(
            PropertyInfo property)
        {
            if (property.GetCustomAttribute(typeof(DisplayNameAttribute)) is DisplayNameAttribute dn)
                return dn.DisplayName;

            return NicifyName(property.Name);
        }

        static string NicifyName(
            string name)
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

        static PropertyGroupKind GetGroupKind(Type propertyType)
        {
            if (propertyType == typeof(Vector2))
                return PropertyGroupKind.Vector2;

            if (propertyType == typeof(Vector3))
                return PropertyGroupKind.Vector3;

            if (propertyType == typeof(Vector4))
                return PropertyGroupKind.Vector4;

            if (propertyType == typeof(Color))
                return PropertyGroupKind.Color;

            return PropertyGroupKind.None;
        }

        static PropertyComponent[] GetComponents(PropertyGroupKind groupKind)
        {
            switch (groupKind)
            {
                case PropertyGroupKind.Vector2:
                    return new[] { PropertyComponent.X, PropertyComponent.Y };

                case PropertyGroupKind.Vector3:
                    return new[] { PropertyComponent.X, PropertyComponent.Y, PropertyComponent.Z };

                case PropertyGroupKind.Vector4:
                    return new[] { PropertyComponent.X, PropertyComponent.Y, PropertyComponent.Z, PropertyComponent.W };

                case PropertyGroupKind.Color:
                    return new[] { PropertyComponent.R, PropertyComponent.G, PropertyComponent.B, PropertyComponent.A };

                default:
                    return Array.Empty<PropertyComponent>();
            }
        }
    }
}
