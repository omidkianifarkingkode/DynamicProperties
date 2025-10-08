using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicProperty.Editor
{
    public sealed class ReflectionMetadataResolver : IPropertyMetadataResolver
    {
        readonly Type _enumType;
        readonly Dictionary<int, PropertyMetadata> _cache = new();

        public ReflectionMetadataResolver(Type enumType) { _enumType = enumType; }
        public Type BoundEnumType => _enumType;

        public PropertyMetadata Get(int id)
        {
            if (_enumType == null) return null;
            if (_cache.TryGetValue(id, out var m)) return m;

            string name = Enum.GetName(_enumType, id);
            if (name == null) return null;

            var field = _enumType.GetField(name);
            var meta = new PropertyMetadata
            {
                Type = field.GetCustomAttribute(typeof(PropertyTypeAttribute)) is PropertyTypeAttribute t ? t.Type : PropertyValueType.Int,
                DisplayName = field.GetCustomAttribute(typeof(DisplayNameAttribute)) is DisplayNameAttribute dn ? dn.DisplayName : name,
                EnumType = field.GetCustomAttribute(typeof(PropertyEnumAttribute)) is PropertyEnumAttribute pe ? pe.EnumType : null
            };
            if (field.GetCustomAttribute(typeof(MinMaxAttribute)) is MinMaxAttribute mm) { meta.Min = mm.Min; meta.Max = mm.Max; }
            if (field.GetCustomAttribute(typeof(StepAttribute)) is StepAttribute st) { meta.Step = st.Step; }

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
