using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Property64))]
public class Property64Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keyProp = property.FindPropertyRelative("key");
        var rawProp = property.FindPropertyRelative("rawValue");

        EditorGUI.BeginProperty(position, label, property);
        PropertyDrawerUtil.SplitKeyValueRects(position, out var keyRect, out var valueRect);

        // key field first
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        bool keyChanged = EditorGUI.EndChangeCheck();

        var key = (PropertyKey)keyProp.intValue;
        if (keyChanged)
        {
            rawProp.longValue = 0L;             // FIX: 64-bit reset
            EditorGUI.EndProperty();
            return;
        }

        if (key == PropertyKey.None)
        {
            EditorGUI.HelpBox(valueRect, "No property key selected!", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        var meta = PropertyMetadataCache.Get(key);

        switch (meta.Type)
        {
            case PropertyType.Float:
                {
                    var u = new ValueUnion64 { raw = rawProp.longValue };
                    u.asFloat = PropertyDrawerUtil.DrawFloat(valueRect, u.asFloat, meta.Min, meta.Max, meta.Step);
                    rawProp.longValue = u.raw;
                    break;
                }
            case PropertyType.Int:
                {
                    var u = new ValueUnion64 { raw = rawProp.longValue };
                    int v = PropertyDrawerUtil.DrawInt(valueRect, u.asInt, meta.Min, meta.Max, meta.Step);
                    u.asInt = v;
                    rawProp.longValue = u.raw;
                    break;
                }
            case PropertyType.Bool:
                {
                    var u = new ValueUnion64 { raw = rawProp.longValue };
                    bool v = PropertyDrawerUtil.DrawBool(valueRect, u.asBool);
                    u.asBool = v;
                    rawProp.longValue = u.raw;
                    break;
                }
            case PropertyType.Double:
                {
                    var u = new ValueUnion64 { raw = rawProp.longValue };
                    double v = EditorGUI.DoubleField(valueRect, u.asDouble);
                    u.asDouble = v;
                    rawProp.longValue = u.raw;
                    break;
                }
            case PropertyType.Long:
                {
                    long v = rawProp.longValue;
                    v = EditorGUI.LongField(valueRect, v);
                    rawProp.longValue = v;
                    break;
                }
            case PropertyType.DateTime:
                {
                    float stepSec = (meta.Step.HasValue && meta.Step.Value > 0f) ? meta.Step.Value : 3600f;
                    long ticks = PropertyDrawerUtil.DrawDateTimeTicks(valueRect, rawProp.longValue, stepSec);
                    rawProp.longValue = ticks;
                    break;
                }
            case PropertyType.TimeSpan:
                {
                    float stepSec = (meta.Step.HasValue && meta.Step.Value > 0f) ? meta.Step.Value : 60f;
                    long ticks = PropertyDrawerUtil.DrawTimeSpanTicks(valueRect, rawProp.longValue, stepSec, meta.Min, meta.Max);
                    rawProp.longValue = ticks;
                    break;
                }
            case PropertyType.Enum:
                {
                    var u = new ValueUnion64 { raw = rawProp.longValue };
                    int v = PropertyDrawerUtil.DrawEnum(valueRect, u.asInt, meta.EnumType);
                    u.asInt = v;
                    rawProp.longValue = u.raw;
                    break;
                }
            default:
                EditorGUI.HelpBox(valueRect, $"Type not handled in 64: {meta.Type}", MessageType.Warning);
                break;
        }

        EditorGUI.EndProperty();
    }
}
