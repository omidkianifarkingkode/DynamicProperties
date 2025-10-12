
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    [CustomPropertyDrawer(typeof(PropertySet))]
    public class PropertySetDrawer : PropertyDrawer
    {
        private enum Backing { Bit32, Bit64 }

        private struct Row
        {
            public Backing Bin;            // which internal list this row belongs to
            public bool IsGroup;           // grouped 32-bit float row (Vector/Color)
            public PropertyGroupKind Kind; // None / Vector2 / Vector3 / Color
            public string Label;           // group label or single display name
            public string GroupKey;        // non-null for groups
            public List<int> Indices;      // indices into the corresponding list (_items32/_items64)
            public bool IsDuplicate;       // highlight duplicate
            public float Height;
        }

        // ---------- Routing by type (no bitness) ----------
        private static bool Is32Type(PropertyValueType t) =>
            t == PropertyValueType.Int
         || t == PropertyValueType.Float
         || t == PropertyValueType.Bool
         || t == PropertyValueType.Enum;

        private static bool Is64Type(PropertyValueType t) =>
            t == PropertyValueType.Long
         || t == PropertyValueType.Double
         || t == PropertyValueType.DateTime
         || t == PropertyValueType.TimeSpan;

        // ---------- Height ----------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;
            if (resolver == null) return EditorGUIUtility.singleLineHeight * 2f;

            var (items32, items64) = GetLists(property);
            if (items32 == null || items64 == null) return EditorGUIUtility.singleLineHeight * 2f;

            BuildRowsUnified(property, resolver, out var rows, out bool hasAnyDup);

            float h = EditorGUIUtility.singleLineHeight + 4f; // title
            if (hasAnyDup) h += EditorGUIUtility.singleLineHeight * 1.5f + 4f;
            foreach (var r in rows) h += r.Height + 2f;
            return h + 6f;
        }

        // ---------- GUI ----------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;
            if (resolver == null)
            {
                EditorGUI.HelpBox(position, "Bind your PropertyId enum in Project Settings → Dynamic Properties.", MessageType.Warning);
                return;
            }

            var (items32, items64) = GetLists(property);
            if (items32 == null || items64 == null)
            {
                EditorGUI.HelpBox(position, "PropertySet: internal lists not found.", MessageType.Error);
                return;
            }

            float y = position.y;

            // Title + single Add
            var titleRect = new Rect(position.x, y, position.width - 90f, EditorGUIUtility.singleLineHeight);
            var addRect = new Rect(position.x + position.width - 90f, y, 90f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(titleRect, ObjectNames.NicifyVariableName(property.displayName), EditorStyles.boldLabel);
            if (GUI.Button(addRect, "+ Add", EditorStyles.miniButton))
            {
                ShowAddMenuUnified(items32, items64, resolver);
                SetDefaultValueForNewProperty(items32, resolver);
            }
            y += titleRect.height + 4f;

            // Rows
            BuildRowsUnified(property, resolver, out var rows, out bool hasAnyDup);
            if (hasAnyDup)
            {
                var warn = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(warn, "Duplicate properties detected. Remove extras to avoid undefined behavior.", MessageType.Warning);
                y += warn.height + 4f;
            }

            foreach (var row in rows)
            {
                var r = new Rect(position.x, y, position.width, row.Height);

                if (row.IsDuplicate)
                    EditorGUI.DrawRect(new Rect(r.x, r.y + 1f, r.width, r.height - 2f), new Color(1f, 0.85f, 0.85f, 0.35f));

                if (row.IsGroup) DrawGroupRow32(r, row, items32, resolver);
                else
                {
                    if (row.Bin == Backing.Bit32) DrawSingleRow32(r, items32, row.Indices[0], resolver);
                    else DrawSingleRow64(r, items64, row.Indices[0], resolver);
                }

                y += row.Height + 2f;
            }
        }

        // ---------- Build unified rows (group 32-bit float groups; singles: 32 then 64) ----------
        private void BuildRowsUnified(SerializedProperty property, IPropertyMetadataResolver resolver,
                                      out List<Row> rows, out bool hasAnyDup)
        {
            var (items32, items64) = GetLists(property);
            rows = new List<Row>();
            hasAnyDup = false;

            bool dup32 = HasDuplicates(items32, out var map32);
            bool dup64 = HasDuplicates(items64, out var map64);
            var ids32 = CollectIds(items32);
            var ids64 = CollectIds(items64);
            bool cross = ids32.Overlaps(ids64);
            hasAnyDup = dup32 || dup64 || cross;

            var consumed32 = new HashSet<int>();
            var groupBuckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            // 32-bit group buckets (float + group name)
            for (int i = 0; i < items32.arraySize; i++)
            {
                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                if (meta == null) continue;

                if (meta.Type == PropertyValueType.Float && !string.IsNullOrEmpty(meta.GroupName))
                {
                    if (!groupBuckets.TryGetValue(meta.GroupName, out var list))
                    {
                        list = new List<int>();
                        groupBuckets[meta.GroupName] = list;
                    }
                    list.Add(i);
                }
            }

            // Group rows
            foreach (var kvp in groupBuckets)
            {
                var indices = kvp.Value;
                if (indices.Count < 2) continue;

                var firstMeta = resolver.Get(items32.GetArrayElementAtIndex(indices[0]).FindPropertyRelative("id").intValue);
                var kind = firstMeta?.GroupKind ?? PropertyGroupKind.None;

                var ordered = kind == PropertyGroupKind.Color
                            ? SortByColorChannels(items32, indices, resolver, out _)
                            : SortByAxis(items32, indices, resolver);

                int take = Mathf.Clamp(ordered.Count, 2, 4);
                var draw = ordered.GetRange(0, take);
                foreach (var idx in draw) consumed32.Add(idx);

                bool d = draw.Any(ix =>
                {
                    int id = items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue;
                    return (map32.TryGetValue(id, out var c) && c > 1) || ids64.Contains(id);
                });

                rows.Add(new Row
                {
                    Bin = Backing.Bit32,
                    IsGroup = true,
                    Kind = kind,
                    Label = kvp.Key,
                    GroupKey = kvp.Key,
                    Indices = draw,
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight
                });
            }

            // 32-bit singles (non-grouped or unmatched)
            for (int i = 0; i < items32.arraySize; i++)
            {
                if (consumed32.Contains(i)) continue;

                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                bool d = (map32.TryGetValue(id, out var c) && c > 1) || ids64.Contains(id);

                rows.Add(new Row
                {
                    Bin = Backing.Bit32,
                    IsGroup = false,
                    Kind = PropertyGroupKind.None,
                    Label = null,
                    GroupKey = null,
                    Indices = new List<int> { i },
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight
                });
            }

            // 64-bit singles
            for (int i = 0; i < items64.arraySize; i++)
            {
                int id = items64.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                bool d = (map64.TryGetValue(id, out var c) && c > 1) || ids32.Contains(id);

                rows.Add(new Row
                {
                    Bin = Backing.Bit64,
                    IsGroup = false,
                    Kind = PropertyGroupKind.None,
                    Label = null,
                    GroupKey = null,
                    Indices = new List<int> { i },
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight
                });
            }
        }

        // ---------- Add menu (single entry point; routes by PropertyValueType) ----------
        private void ShowAddMenuUnified(SerializedProperty items32, SerializedProperty items64, IPropertyMetadataResolver resolver)
        {
            // existing across both
            var existing = CollectIds(items32);
            existing.UnionWith(CollectIds(items64));

            var names = resolver.GetAllNames();
            var values = resolver.GetAllValues();

            var groups32 = new Dictionary<string, (PropertyGroupKind kind, List<(int id, string disp)>)>(StringComparer.OrdinalIgnoreCase);
            var singles = new List<(int id, string disp, PropertyValueType type)>();

            for (int i = 0; i < values.Length; i++)
            {
                int id = values[i];
                if (existing.Contains(id)) continue;

                var meta = resolver.Get(id);
                if (meta == null) continue;

                if (meta.HiddenInEditor || id == 0) continue;

                string disp = meta.DisplayName ?? names[i];

                // Group only applies to 32-bit float items
                if (meta.Type == PropertyValueType.Float && !string.IsNullOrEmpty(meta.GroupName))
                {
                    if (!groups32.TryGetValue(meta.GroupName, out var entry))
                        entry = (meta.GroupKind, new List<(int, string)>());
                    entry.Item2.Add((id, disp));
                    groups32[meta.GroupName] = entry;
                }
                else
                {
                    singles.Add((id, disp, meta.Type));
                }
            }

            var menu = new GenericMenu();

            // 32-bit groups (display if any member missing — enforced by existing filter)
            foreach (var kvp in groups32.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var label = kvp.Key;
                var kind = kvp.Value.kind;
                if (kind == PropertyGroupKind.Color) label += "  (Color)";
                else if (kind == PropertyGroupKind.Vector2) label += "  (Vector2)";
                else if (kind == PropertyGroupKind.Vector3) label += "  (Vector3)";

                var items = kvp.Value.Item2;
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    foreach (var (id, _) in items) Add32(items32, id, 0);
                    items32.serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() > 0 && singles.Count > 0) menu.AddSeparator("");

            // Singles: route by type
            foreach (var entry in singles.OrderBy(s => s.disp, StringComparer.OrdinalIgnoreCase))
            {
                bool is64 = Is64Type(entry.type);
                var content = new GUIContent(entry.disp + (is64 ? "  (64)" : "  (32)"));
                menu.AddItem(content, false, () =>
                {
                    if (is64) Add64(items64, entry.id, 0L);
                    else Add32(items32, entry.id, 0);
                    items32.serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No unused properties"));

            menu.ShowAsContext();
        }

        // ---------- 32-bit grouped row ----------
        private void DrawGroupRow32(Rect r, Row row, SerializedProperty items32, IPropertyMetadataResolver resolver)
        {
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);
            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                var ids = row.Indices.Select(ix => items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue);
                RemoveByIds(items32, ids);
                items32.serializedObject.ApplyModifiedProperties();
                return;
            }

            float third = r.width / 3f;
            var labelRect = new Rect(r.x, r.y, third, r.height);
            var fieldRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);
            EditorGUI.LabelField(labelRect, row.Label);

            int comp = row.Indices.Count;
            var vals = new float[comp];
            for (int i = 0; i < comp; i++)
            {
                var e = items32.GetArrayElementAtIndex(row.Indices[i]);
                int raw = e.FindPropertyRelative("rawValue").intValue;
                vals[i] = new ValueUnion32 { raw = raw }.asFloat;
            }

            if (row.Kind == PropertyGroupKind.Color && comp >= 3)
            {
                var ordered = SortByColorChannels(items32, row.Indices, resolver, out int use);
                var rgba = new float[4] { 1, 1, 1, 1 };
                for (int i = 0; i < use; i++)
                {
                    var e = items32.GetArrayElementAtIndex(ordered[i]);
                    int raw = e.FindPropertyRelative("rawValue").intValue;
                    rgba[i] = new ValueUnion32 { raw = raw }.asFloat;
                }

                EditorGUI.BeginChangeCheck();
                var col = new Color(
                    use > 0 ? rgba[0] : 1f,
                    use > 1 ? rgba[1] : 1f,
                    use > 2 ? rgba[2] : 1f,
                    use > 3 ? rgba[3] : 1f
                );
                col = EditorGUI.ColorField(fieldRect, GUIContent.none, col, true, true, false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (use > 0) WriteFloat(items32, ordered[0], col.r);
                    if (use > 1) WriteFloat(items32, ordered[1], col.g);
                    if (use > 2) WriteFloat(items32, ordered[2], col.b);
                    if (use > 3) WriteFloat(items32, ordered[3], col.a);
                    items32.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                switch (comp)
                {
                    case 2:
                        {
                            var v = new Vector2(vals[0], vals[1]);
                            v = EditorGUI.Vector2Field(fieldRect, GUIContent.none, v);
                            vals[0] = v.x; vals[1] = v.y;
                            break;
                        }
                    case 3:
                        {
                            var v = new Vector3(vals[0], vals[1], vals[2]);
                            v = EditorGUI.Vector3Field(fieldRect, GUIContent.none, v);
                            vals[0] = v.x; vals[1] = v.y; vals[2] = v.z;
                            break;
                        }
                    default:
                        {
                            var v = new Vector4(vals[0], vals[1], vals[2], vals[3]);
                            v = EditorGUI.Vector4Field(fieldRect, GUIContent.none, v);
                            vals[0] = v.x; vals[1] = v.y; vals[2] = v.z; vals[3] = v.w;
                            break;
                        }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < comp; i++) WriteFloat(items32, row.Indices[i], vals[i]);
                    items32.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // ---------- 32-bit single ----------
        private void DrawSingleRow32(Rect r, SerializedProperty items32, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items32.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id) ?? new PropertyMetadata { Type = PropertyValueType.Int };
            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            EditorGUI.LabelField(keyRect, label);

            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                items32.DeleteArrayElementAtIndex(index);
                items32.serializedObject.ApplyModifiedProperties();
                return;
            }

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
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (32): {meta.Type}", MessageType.None);
                    break;
            }
        }

        // ---------- 64-bit single ----------
        private void DrawSingleRow64(Rect r, SerializedProperty items64, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items64.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id) ?? new PropertyMetadata { Type = PropertyValueType.Long };
            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            EditorGUI.LabelField(keyRect, label);

            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                items64.DeleteArrayElementAtIndex(index);
                items64.serializedObject.ApplyModifiedProperties();
                return;
            }

            long raw = rawProp.longValue;
            var u = new ValueUnion64 { raw = raw };

            switch (meta.Type)
            {
                case PropertyValueType.Double:
                    {
                        double v = EditorGUI.DoubleField(valRect, u.asDouble);
                        u.asDouble = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Long:
                    {
                        long v = EditorGUI.LongField(valRect, u.raw);
                        u.raw = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Int:
                    {
                        int v = PropertyDrawerUtil.DrawInt(valRect, u.asInt, meta.Min, meta.Max, meta.Step);
                        u.asInt = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Float:
                    {
                        float v = PropertyDrawerUtil.DrawFloat(valRect, u.asFloat, meta.Min, meta.Max, meta.Step);
                        u.asFloat = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.Bool:
                    {
                        bool v = PropertyDrawerUtil.DrawBool(valRect, u.asBool);
                        u.asBool = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
                case PropertyValueType.DateTime:
                    {
                        long ticks = u.raw;
                        DateTime dt = new DateTime(ticks, DateTimeKind.Utc);
                        string txt = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        valRect.width -= 53;
                        string newTxt = EditorGUI.DelayedTextField(valRect, txt);
                        if (DateTime.TryParse(newTxt, out var parsed)) dt = parsed;

                        // Optional stepper (hour default)
                        float stepSec = meta.Step.HasValue && meta.Step.Value > 0f ? meta.Step.Value : 60;
                        var minusR = new Rect(valRect.xMax + 2, r.y, 25, r.height);
                        var plusR = new Rect(valRect.xMax + 28, r.y, 25, r.height);
                        if (GUI.Button(minusR, "-")) dt = dt.AddSeconds(-stepSec);
                        if (GUI.Button(plusR, "+")) dt = dt.AddSeconds(stepSec);

                        rawProp.longValue = dt.Ticks;
                        break;
                    }
                case PropertyValueType.TimeSpan:
                    {
                        TimeSpan ts;
                        try { ts = new TimeSpan(u.raw); } catch { ts = TimeSpan.Zero; }

                        string fmt = ts.Days != 0 ? @"d\.hh\:mm\:ss" : @"hh\:mm\:ss";
                        EditorGUI.BeginChangeCheck();
                        valRect.width -= 53;
                        string txt = EditorGUI.DelayedTextField(valRect, ts.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture));
                        if (EditorGUI.EndChangeCheck() && TimeSpan.TryParse(txt, out var parsed))
                            ts = parsed;

                        double stepSec = meta.Step.HasValue && meta.Step.Value > 0f ? meta.Step.Value : 5;
                        var minusR = new Rect(valRect.xMax + 2, r.y, 25, r.height);
                        var plusR = new Rect(valRect.xMax + 28, r.y, 25, r.height);
                        if (GUI.Button(minusR, "-")) ts -= TimeSpan.FromSeconds(stepSec);
                        if (GUI.Button(plusR, "+")) ts += TimeSpan.FromSeconds(stepSec);

                        if (meta.Min.HasValue && meta.Max.HasValue)
                        {
                            double s = Mathf.Clamp((float)ts.TotalSeconds, meta.Min.Value, meta.Max.Value);
                            ts = TimeSpan.FromSeconds(s);
                        }

                        rawProp.longValue = ts.Ticks;
                        break;
                    }
                case PropertyValueType.Enum:
                    {
                        int enumVal = u.asInt;
                        var enumType = meta.EnumType;
                        if (enumType == null) { EditorGUI.HelpBox(valRect, "Enum type not defined!", MessageType.Warning); break; }

                        bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
                        if (isFlags)
                        {
                            enumVal = EditorGUI.MaskField(valRect, enumVal, Enum.GetNames(enumType));
                            int all = 0; foreach (var v in Enum.GetValues(enumType)) all |= Convert.ToInt32(v);
                            enumVal &= all;
                        }
                        else
                        {
                            var names = Enum.GetNames(enumType);
                            var values = (Array)Enum.GetValues(enumType);
                            int[] ints = new int[values.Length];
                            for (int i = 0; i < values.Length; i++) ints[i] = Convert.ToInt32(values.GetValue(i));
                            enumVal = EditorGUI.IntPopup(valRect, enumVal, names, ints);
                        }
                        u.asInt = enumVal;
                        rawProp.longValue = u.raw;
                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (64): {meta.Type}", MessageType.None);
                    break;
            }
        }

        // ---------- Helpers ----------
        private static (SerializedProperty items32, SerializedProperty items64) GetLists(SerializedProperty property)
            => (property.FindPropertyRelative("_items32"), property.FindPropertyRelative("_items64"));

        private static HashSet<int> CollectIds(SerializedProperty list)
        {
            var set = new HashSet<int>();
            for (int i = 0; i < list.arraySize; i++)
                set.Add(list.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue);
            return set;
        }

        private static bool HasDuplicates(SerializedProperty list, out Dictionary<int, int> counts)
        {
            counts = new Dictionary<int, int>();
            for (int i = 0; i < list.arraySize; i++)
            {
                int id = list.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                counts.TryGetValue(id, out var c);
                counts[id] = c + 1;
            }
            return counts.Any(k => k.Value > 1);
        }

        private static void Add32(SerializedProperty list, int id, int raw = 0)
        {
            int newIndex = list.arraySize;
            list.arraySize++;
            var e = list.GetArrayElementAtIndex(newIndex);
            e.FindPropertyRelative("id").intValue = id;
            e.FindPropertyRelative("rawValue").intValue = raw;
        }

        private static void Add64(SerializedProperty list, int id, long raw = 0L)
        {
            int newIndex = list.arraySize;
            list.arraySize++;
            var e = list.GetArrayElementAtIndex(newIndex);
            e.FindPropertyRelative("id").intValue = id;
            e.FindPropertyRelative("rawValue").longValue = raw;
        }

        private static void RemoveByIds(SerializedProperty list, IEnumerable<int> ids)
        {
            var idSet = new HashSet<int>(ids);
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                int id = list.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                if (idSet.Contains(id)) list.DeleteArrayElementAtIndex(i);
            }
        }

        private List<int> SortByAxis(SerializedProperty items32, List<int> indices, IPropertyMetadataResolver resolver)
        {
            int Score(string name)
            {
                if (string.IsNullOrEmpty(name)) return 99;
                var n = name.Trim().ToLowerInvariant();
                if (n.EndsWith(" x") || n.EndsWith(".x") || n.EndsWith("x")) return 0;
                if (n.EndsWith(" y") || n.EndsWith(".y") || n.EndsWith("y")) return 1;
                if (n.EndsWith(" z") || n.EndsWith(".z") || n.EndsWith("z")) return 2;
                if (n.EndsWith(" w") || n.EndsWith(".w") || n.EndsWith("w")) return 3;
                return 99;
            }

            return indices.OrderBy(i =>
            {
                var e = items32.GetArrayElementAtIndex(i);
                int id = e.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                var disp = meta?.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id);
                return Score(disp);
            }).ToList();
        }

        private List<int> SortByColorChannels(SerializedProperty items32, List<int> indices, IPropertyMetadataResolver resolver, out int use)
        {
            int Score(string name)
            {
                if (string.IsNullOrEmpty(name)) return 99;
                var n = name.Trim().ToLowerInvariant();
                if (n.EndsWith(" r") || n.EndsWith(".r") || n.EndsWith("r")) return 0;
                if (n.EndsWith(" g") || n.EndsWith(".g") || n.EndsWith("g")) return 1;
                if (n.EndsWith(" b") || n.EndsWith(".b") || n.EndsWith("b")) return 2;
                if (n.EndsWith(" a") || n.EndsWith(".a") || n.EndsWith("a")) return 3;
                return 99;
            }

            var ordered = indices.OrderBy(i =>
            {
                var e = items32.GetArrayElementAtIndex(i);
                int id = e.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                var disp = meta?.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id);
                return Score(disp);
            }).ToList();

            use = Mathf.Min(4, ordered.Count);
            return ordered;
        }

        private static void WriteFloat(SerializedProperty items32, int itemIndex, float value)
        {
            var e = items32.GetArrayElementAtIndex(itemIndex);
            var u = new ValueUnion32 { asFloat = value };
            e.FindPropertyRelative("rawValue").intValue = u.raw;
        }

        private void SetDefaultValueForNewProperty(SerializedProperty items32, IPropertyMetadataResolver resolver)
        {
            // Iterate through all items (32-bit properties)
            for (int i = 0; i < items32.arraySize; i++)
            {
                var item = items32.GetArrayElementAtIndex(i);
                int id = item.FindPropertyRelative("id").intValue;

                // Fetch the metadata for this property
                var meta = resolver.Get(id);
                if (meta != null && meta.DefaultValue != null)
                {
                    var rawValueProperty = item.FindPropertyRelative("rawValue");

                    // Check if the property has a default value and handle by type
                    switch (meta.Type)
                    {
                        case PropertyValueType.Bool:
                            rawValueProperty.intValue = (bool)meta.DefaultValue ? 1 : 0;
                            break;
                        case PropertyValueType.Int:
                            rawValueProperty.intValue = (int)meta.DefaultValue;
                            break;
                        case PropertyValueType.Double:
                            rawValueProperty.doubleValue = (double)meta.DefaultValue;
                            break;
                        case PropertyValueType.Long:
                            rawValueProperty.longValue = (long)meta.DefaultValue;
                            break;
                        case PropertyValueType.Enum:
                            var enumValue = (Enum)meta.DefaultValue;
                            rawValueProperty.intValue = Convert.ToInt32(enumValue);
                            break;
                        case PropertyValueType.Float:
                            rawValueProperty.floatValue = (float)meta.DefaultValue;
                            break;

                        // Handle other types (e.g., Vectors, Color, DateTime, TimeSpan)
                        default:
                            Debug.LogWarning($"Unhandled PropertyValueType: {meta.Type}");
                            break;
                    }
                }
            }

            items32.serializedObject.ApplyModifiedProperties();
        }


    }
}