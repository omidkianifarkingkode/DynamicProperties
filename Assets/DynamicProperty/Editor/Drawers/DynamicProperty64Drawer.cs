using System;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    [CustomPropertyDrawer(typeof(DynamicProperty64))]
    public class DynamicProperty64Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative("id");
            var rawProp = property.FindPropertyRelative("rawValue");

            EditorGUI.BeginProperty(position, label, property);
            PropertyDrawerUtil.SplitKeyValueRects(position, out var keyRect, out var valRect);

            PropertyMetadataRegistry.EnsureBound();
            var res = PropertyMetadataRegistry.Resolver;
            if (res == null)
            {
                EditorGUI.HelpBox(position, "Bind your PropertyId enum in Project Settings → Dynamic Properties.", MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            // Key popup
            var names = res.GetAllNames();
            var vals = res.GetAllValues();
            int curId = idProp.intValue;
            int idx = Array.IndexOf(vals, curId);
            if (idx < 0) idx = 0;
            idx = EditorGUI.Popup(keyRect, idx, names);
            idProp.intValue = vals.Length > 0 ? vals[Mathf.Clamp(idx, 0, vals.Length - 1)] : 0;

            if (curId != idProp.intValue)
            {
                rawProp.longValue = 0L;
                EditorGUI.EndProperty();
                return;
            }

            var meta = res.Get(curId) ?? new PropertyMetadata { Type = PropertyValueType.Int };

            switch (meta.Type)
            {
                case PropertyValueType.Float:
                    {
                        var u = new ValueUnion64 { raw = rawProp.longValue };
                        u.asFloat = PropertyDrawerUtil.DrawFloat(valRect, u.asFloat, meta.Min, meta.Max, meta.Step);
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Int:
                    {
                        var u = new ValueUnion64 { raw = rawProp.longValue };
                        u.asInt = PropertyDrawerUtil.DrawInt(valRect, u.asInt, meta.Min, meta.Max, meta.Step);
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Bool:
                    {
                        var u = new ValueUnion64 { raw = rawProp.longValue };
                        u.asBool = PropertyDrawerUtil.DrawBool(valRect, u.asBool);
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Double:
                    {
                        var u = new ValueUnion64 { raw = rawProp.longValue };
                        u.asDouble = EditorGUI.DoubleField(valRect, u.asDouble);
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Long:
                    {
                        long v = EditorGUI.LongField(valRect, rawProp.longValue);
                        rawProp.longValue = v;
                        break;
                    }
                case PropertyValueType.DateTime:
                    {
                        float step = meta.Step ?? 3600f;
                        long ticks = PropertyDrawerUtil.DrawDateTimeTicks(valRect, rawProp.longValue, step);
                        rawProp.longValue = ticks;
                        break;
                    }
                case PropertyValueType.TimeSpan:
                    {
                        float step = meta.Step ?? 60f;
                        long ticks = PropertyDrawerUtil.DrawTimeSpanTicks(valRect, rawProp.longValue, step, meta.Min, meta.Max);
                        rawProp.longValue = ticks;
                        break;
                    }
                case PropertyValueType.Enum:
                    {
                        var u = new ValueUnion64 { raw = rawProp.longValue };
                        int v = PropertyDrawerUtil.DrawEnum(valRect, u.asInt, meta.EnumType);
                        u.asInt = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (64): {meta.Type}", MessageType.Warning);
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}
