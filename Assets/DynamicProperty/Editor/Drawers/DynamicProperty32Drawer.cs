using System;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    [CustomPropertyDrawer(typeof(DynamicProperty32))]
    public class DynamicProperty32Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative("id");
            var rawProp = property.FindPropertyRelative("rawValue");

            EditorGUI.BeginProperty(position, label, property);
            PropertyDrawerUtil.SplitKeyValueRects(position, out var keyRect, out var valRect);

            // Key dropdown from bound enum
            PropertyMetadataRegistry.EnsureBound();
            var res = PropertyMetadataRegistry.Resolver;
            if (res == null)
            {
                EditorGUI.HelpBox(position, "Bind your PropertyId enum in Project Settings → Dynamic Properties.", MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            // Draw key popup
            var names = res.GetAllNames();
            var vals = res.GetAllValues();
            int curId = idProp.intValue;

            int idx = Array.IndexOf(vals, curId);
            if (idx < 0) idx = 0;
            idx = EditorGUI.Popup(keyRect, idx, names);
            idProp.intValue = vals.Length > 0 ? vals[Mathf.Clamp(idx, 0, vals.Length - 1)] : 0;

            // If key changed, reset raw
            if (curId != idProp.intValue)
            {
                rawProp.intValue = 0;
                EditorGUI.EndProperty();
                return;
            }

            // Draw value by metadata
            var meta = res.Get(curId) ?? new PropertyMetadata { Type = PropertyValueType.Int };
            switch (meta.Type)
            {
                case PropertyValueType.Float:
                    {
                        var u = new ValueUnion32 { raw = rawProp.intValue };
                        u.asFloat = PropertyDrawerUtil.DrawFloat(valRect, u.asFloat, meta.Min, meta.Max, meta.Step);
                        rawProp.intValue = u.raw;
                        break;
                    }
                case PropertyValueType.Int:
                    {
                        int v = PropertyDrawerUtil.DrawInt(valRect, rawProp.intValue, meta.Min, meta.Max, meta.Step);
                        rawProp.intValue = v;
                        break;
                    }
                case PropertyValueType.Bool:
                    {
                        bool v = PropertyDrawerUtil.DrawBool(valRect, rawProp.intValue != 0);
                        rawProp.intValue = v ? 1 : 0;
                        break;
                    }
                case PropertyValueType.Enum:
                    {
                        int v = PropertyDrawerUtil.DrawEnum(valRect, rawProp.intValue, meta.EnumType);
                        rawProp.intValue = v;
                        break;
                    }
                case PropertyValueType.DateTimeShort:
                    {
                        // minutes since epoch as int
                        long ticks = DateTime.UnixEpoch.AddMinutes(rawProp.intValue).Ticks;
                        ticks = PropertyDrawerUtil.DrawDateTimeTicks(valRect, ticks, meta.Step ?? 60f);
                        var minutes = (int)Mathf.Clamp((float)TimeSpan.FromTicks(ticks).TotalMinutes, int.MinValue, int.MaxValue);
                        rawProp.intValue = minutes;
                        break;
                    }
                case PropertyValueType.TimeSpanShort:
                    {
                        // minutes in int
                        long ticks = TimeSpan.FromMinutes(rawProp.intValue).Ticks;
                        ticks = PropertyDrawerUtil.DrawTimeSpanTicks(valRect, ticks, meta.Step ?? 5f, meta.Min, meta.Max);
                        var minutes = (int)Mathf.Clamp((float)TimeSpan.FromTicks(ticks).TotalMinutes, int.MinValue, int.MaxValue);
                        rawProp.intValue = minutes;
                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (32): {meta.Type}", MessageType.Warning);
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}
