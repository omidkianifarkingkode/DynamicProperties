using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Property32))]
public class Property32Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keyProp = property.FindPropertyRelative("key");
        var rawProp = property.FindPropertyRelative("rawValue");

        EditorGUI.BeginProperty(position, label, property);
        PropertyDrawerUtil.SplitKeyValueRects(position, out var keyRect, out var valRect);

        // draw key first, then read current metadata
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        bool keyChanged = EditorGUI.EndChangeCheck();

        var key = (PropertyKey)keyProp.intValue;
        if (keyChanged)
        {
            rawProp.intValue = 0;               // 32-bit reset
            EditorGUI.EndProperty();
            return;
        }

        if (key == PropertyKey.None)
        {
            EditorGUI.HelpBox(valRect, "No property key selected!", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        var meta = PropertyMetadataCache.Get(key);

        // draw value by type
        switch (meta.Type)
        {
            case PropertyType.Float:
                {
                    // reinterpret int bits as float via ValueUnion32
                    var u = new ValueUnion32 { raw = rawProp.intValue };
                    u.asFloat = PropertyDrawerUtil.DrawFloat(valRect, u.asFloat, meta.Min, meta.Max, meta.Step);
                    rawProp.intValue = u.raw;
                    break;
                }
            case PropertyType.Int:
                {
                    int v = rawProp.intValue;
                    v = PropertyDrawerUtil.DrawInt(valRect, v, meta.Min, meta.Max, meta.Step);
                    rawProp.intValue = v;
                    break;
                }
            case PropertyType.Bool:
                {
                    bool v = rawProp.intValue != 0;
                    v = PropertyDrawerUtil.DrawBool(valRect, v);
                    rawProp.intValue = v ? 1 : 0;
                    break;
                }
            case PropertyType.Enum:
                {
                    int v = rawProp.intValue;
                    v = PropertyDrawerUtil.DrawEnum(valRect, v, meta.EnumType);
                    rawProp.intValue = v;
                    break;
                }
            // If you added DateTimeShort/TimeSpanShort (minutes-based), handle them here.
            default:
                EditorGUI.HelpBox(valRect, $"Type not handled in 32: {meta.Type}", MessageType.Warning);
                break;
        }

        EditorGUI.EndProperty();
    }
}
