using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor {
    //[CustomPropertyDrawer(typeof(PropertySet))]
    public class PropertySetDrawer : PropertyDrawer
    {
        // Visual row model for 32-bit section
        private struct Row32
        {
            public string Label;           // group name or single label
            public string GroupKey;        // null for singles
            public List<int> Indices;      // indices into _items32
            public PropertyGroupKind Kind; // Color/Vector2/Vector3/None
            public bool IsGroup;
            public bool IsDuplicate;
            public float Height;
        }

        // -------------------- Sizing --------------------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;
            if (resolver == null) return EditorGUIUtility.singleLineHeight * 2f;

            var items32 = property.FindPropertyRelative("_items32");
            var items64 = property.FindPropertyRelative("_items64");
            if (items32 == null || items64 == null) return EditorGUIUtility.singleLineHeight * 2f;

            BuildRows32(property, resolver, out var rows32, out var dup32, out var crossDup);
            var dup64 = HasDuplicates(items64, out _);

            float h = 0f;
            // Title
            h += EditorGUIUtility.singleLineHeight + 4f;

            // Warnings
            if (dup32 || dup64 || crossDup)
            {
                // up to two lines of help boxes
                if (dup32) h += EditorGUIUtility.singleLineHeight * 1.5f + 4f;
                if (dup64) h += EditorGUIUtility.singleLineHeight * 1.5f + 4f;
                if (crossDup) h += EditorGUIUtility.singleLineHeight * 1.5f + 4f;
            }

            // 32-bit header
            h += EditorGUIUtility.singleLineHeight + 2f;
            foreach (var r in rows32) h += r.Height + 2f;

            // 64-bit header
            h += EditorGUIUtility.singleLineHeight + 2f;
            // 64 rows each single-line (we don’t group 64 in this drawer)
            for (int i = 0; i < items64.arraySize; i++)
                h += EditorGUIUtility.singleLineHeight + 2f;

            // Bottom padding
            return h + 6f;
        }

        // -------------------- GUI --------------------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;
            if (resolver == null)
            {
                EditorGUI.HelpBox(position, "Bind your PropertyId enum in Project Settings → Dynamic Properties.", MessageType.Warning);
                return;
            }

            var items32 = property.FindPropertyRelative("_items32");
            var items64 = property.FindPropertyRelative("_items64");
            if (items32 == null || items64 == null)
            {
                EditorGUI.HelpBox(position, "PropertySet: internal lists not found.", MessageType.Error);
                return;
            }

            float y = position.y;

            // Title
            var titleRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(titleRect, ObjectNames.NicifyVariableName(property.displayName), EditorStyles.boldLabel);
            y += titleRect.height + 4f;

            // Build 32 rows & detect duplicates
            BuildRows32(property, resolver, out var rows32, out var dup32, out var crossDup);
            bool dup64 = HasDuplicates(items64, out _);

            // Warnings (up to 3)
            if (dup32)
            {
                var warn = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(warn, "Duplicate 32-bit parameters detected. Remove extras to avoid undefined behavior.", MessageType.Warning);
                y += warn.height + 4f;
            }
            if (dup64)
            {
                var warn = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(warn, "Duplicate 64-bit parameters detected. Remove extras to avoid undefined behavior.", MessageType.Warning);
                y += warn.height + 4f;
            }
            if (crossDup)
            {
                var warn = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(warn, "The same PropertyId exists in BOTH 32-bit and 64-bit lists.", MessageType.Warning);
                y += warn.height + 4f;
            }

            // =================== 32-bit section ===================
            DrawSectionHeader(position.x, ref y, position.width, "32-bit Properties", () =>
            {
                ShowAddMenu(items32, items64, resolver, wantBit64: false);
            });

            foreach (var row in rows32)
            {
                var r = new Rect(position.x, y, position.width, row.Height);

                if (row.IsDuplicate)
                {
                    var bg = new Color(1f, 0.85f, 0.85f, 0.35f);
                    EditorGUI.DrawRect(new Rect(r.x, r.y + 1f, r.width, r.height - 2f), bg);
                }

                if (row.IsGroup)
                    DrawGroupRow32(r, items32, row, resolver);
                else
                    DrawSingleRow32(r, items32, row.Indices[0], resolver);

                y += row.Height + 2f;
            }

            // =================== 64-bit section ===================
            DrawSectionHeader(position.x, ref y, position.width, "64-bit Properties", () =>
            {
                ShowAddMenu(items32, items64, resolver, wantBit64: true);
            });

            for (int i = 0; i < items64.arraySize; i++)
            {
                var r = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                DrawSingleRow64(r, items32, items64, i, resolver); // pass items32 to allow cross-remove checks
                y += r.height + 2f;
            }
        }

        // -------------------- Section Header + Add button --------------------
        private void DrawSectionHeader(float x, ref float y, float width, string title, Action onAdd)
        {
            var left = new Rect(x, y, width - 90f, EditorGUIUtility.singleLineHeight);
            var right = new Rect(x + width - 90f, y, 90f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(left, title, EditorStyles.boldLabel);
            if (GUI.Button(right, "+ Add", EditorStyles.miniButton)) onAdd?.Invoke();
            y += EditorGUIUtility.singleLineHeight + 2f;
        }

        // -------------------- Build 32-bit grouped rows --------------------
        private void BuildRows32(SerializedProperty property,
                                 IPropertyMetadataResolver resolver,
                                 out List<Row32> rows,
                                 out bool hasDup32,
                                 out bool hasCrossDup)
        {
            var items32 = property.FindPropertyRelative("_items32");
            var items64 = property.FindPropertyRelative("_items64");
            rows = new List<Row32>();
            hasDup32 = false;
            hasCrossDup = false;

            // duplicate maps
            hasDup32 = HasDuplicates(items32, out var dupMap32);

            // cross-list duplicates
            var ids32 = CollectIds(items32);
            var ids64 = CollectIds(items64);
            hasCrossDup = ids32.Overlaps(ids64);

            // group buckets (by GroupName), only for 32-bit
            var groupBuckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            int count = items32.arraySize;
            for (int i = 0; i < count; i++)
            {
                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                if (meta == null) continue;

                // only include 32-capable items for this section
                if (!meta.Bitness.HasFlag(PropertyBitness.Bit32)) continue;

                if (!string.IsNullOrEmpty(meta.GroupName) && meta.Type == PropertyValueType.Float)
                {
                    if (!groupBuckets.TryGetValue(meta.GroupName, out var list))
                    {
                        list = new List<int>();
                        groupBuckets[meta.GroupName] = list;
                    }
                    list.Add(i);
                }
            }

            var consumed = new HashSet<int>();

            // grouped rows
            foreach (var kvp in groupBuckets)
            {
                var indices = kvp.Value;
                if (indices.Count < 2) continue;

                // get group kind by first member
                var firstMeta = resolver.Get(items32.GetArrayElementAtIndex(indices[0]).FindPropertyRelative("id").intValue);
                var kind = firstMeta?.GroupKind ?? PropertyGroupKind.None;

                // order by axis or channels
                List<int> ordered = kind == PropertyGroupKind.Color
                    ? SortByColorChannels(items32, indices, resolver, out _)
                    : SortByAxis(items32, indices, resolver);

                int take = Mathf.Clamp(ordered.Count, 2, 4);
                var draw = ordered.GetRange(0, take);
                foreach (var idx in draw) consumed.Add(idx);

                bool isDup = draw.Any(ix =>
                {
                    int id = items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue;
                    return dupMap32.TryGetValue(id, out var cnt) && cnt > 1;
                });

                rows.Add(new Row32
                {
                    Label = kvp.Key,
                    GroupKey = kvp.Key,
                    Indices = draw,
                    Kind = kind,
                    IsGroup = true,
                    IsDuplicate = isDup || (hasCrossDup && draw.Any(ix => ids64.Contains(items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue))),
                    Height = EditorGUIUtility.singleLineHeight
                });
            }

            // singles
            for (int i = 0; i < items32.arraySize; i++)
            {
                if (consumed.Contains(i)) continue;

                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                bool isDup = dupMap32.TryGetValue(id, out var cnt) && cnt > 1;

                rows.Add(new Row32
                {
                    Label = null,
                    GroupKey = null,
                    Indices = new List<int> { i },
                    Kind = PropertyGroupKind.None,
                    IsGroup = false,
                    IsDuplicate = isDup || (hasCrossDup && ids64.Contains(id)),
                    Height = EditorGUIUtility.singleLineHeight
                });
            }
        }

        // -------------------- Add menu (32 or 64) --------------------
        private void ShowAddMenu(SerializedProperty items32, SerializedProperty items64,
                                 IPropertyMetadataResolver resolver, bool wantBit64)
        {
            // existing ids across BOTH lists
            var existing = CollectIds(items32);
            existing.UnionWith(CollectIds(items64));

            var names = resolver.GetAllNames();
            var values = resolver.GetAllValues();

            var groups = new Dictionary<string, (PropertyGroupKind kind, List<(int id, string disp)>)>(StringComparer.OrdinalIgnoreCase);
            var singles = new List<(int id, string disp)>();

            for (int i = 0; i < values.Length; i++)
            {
                int id = values[i];
                var meta = resolver.Get(id);
                if (meta == null) continue;

                // filter by bitness target
                bool ok = wantBit64 ? meta.Bitness.HasFlag(PropertyBitness.Bit64)
                                    : meta.Bitness.HasFlag(PropertyBitness.Bit32);
                if (!ok) continue;

                string disp = meta.DisplayName ?? names[i];

                if (!string.IsNullOrEmpty(meta.GroupName) && meta.Type == PropertyValueType.Float && !wantBit64)
                {
                    // groups only make sense / supported visually in 32-bit float section
                    if (!groups.TryGetValue(meta.GroupName, out var entry))
                        entry = (meta.GroupKind, new List<(int, string)>());
                    entry.Item2.Add((id, disp));
                    groups[meta.GroupName] = entry;
                }
                else
                {
                    if (!existing.Contains(id)) singles.Add((id, disp));
                }
            }

            var menu = new GenericMenu();

            if (!wantBit64)
            {
                // groups (only show if any member is missing)
                foreach (var kvp in groups.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var groupName = kvp.Key;
                    var kind = kvp.Value.kind;
                    var items = kvp.Value.Item2;

                    bool anyMissing = items.Any(x => !existing.Contains(x.id));
                    if (!anyMissing) continue;

                    string label = groupName;
                    if (kind == PropertyGroupKind.Color) label += "  (Color)";
                    else if (kind == PropertyGroupKind.Vector2) label += "  (Vector2)";
                    else if (kind == PropertyGroupKind.Vector3) label += "  (Vector3)";

                    menu.AddItem(new GUIContent(label), false, () =>
                    {
                        foreach (var (id, _) in items)
                            if (!existing.Contains(id))
                                Add32(items32, id, 0);
                        items32.serializedObject.ApplyModifiedProperties();
                    });
                }

                if (menu.GetItemCount() > 0 && singles.Count > 0) menu.AddSeparator("");
            }

            // singles
            foreach (var (id, disp) in singles.OrderBy(s => s.disp, StringComparer.OrdinalIgnoreCase))
            {
                menu.AddItem(new GUIContent(disp), false, () =>
                {
                    if (wantBit64) Add64(items64, id, 0L);
                    else Add32(items32, id, 0);
                    items32.serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent(wantBit64 ? "No unused 64-bit parameters" : "No unused 32-bit parameters"));

            menu.ShowAsContext();
        }

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
                counts.TryGetValue(id, out int c);
                counts[id] = c + 1;
            }
            foreach (var kv in counts)
                if (kv.Value > 1) return true;
            return false;
        }

        private static void Add32(SerializedProperty items32, int id, int raw = 0)
        {
            int newIndex = items32.arraySize;
            items32.arraySize++;
            var e = items32.GetArrayElementAtIndex(newIndex);
            e.FindPropertyRelative("id").intValue = id;
            e.FindPropertyRelative("rawValue").intValue = raw;
        }

        private static void Add64(SerializedProperty items64, int id, long raw = 0L)
        {
            int newIndex = items64.arraySize;
            items64.arraySize++;
            var e = items64.GetArrayElementAtIndex(newIndex);
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

        // -------------------- 32-bit: Group Row --------------------
        private void DrawGroupRow32(Rect r, SerializedProperty items32, Row32 row, IPropertyMetadataResolver resolver)
        {
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);
            if (GUI.Button(minusRect, "−", EditorStyles.miniButton))
            {
                var ids = row.Indices.Select(ix => items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue);
                RemoveByIds(items32, ids);
                items32.serializedObject.ApplyModifiedProperties();
                return;
            }

            float labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(r.x, r.y, labelWidth, r.height);
            var fieldRect = new Rect(r.x + labelWidth, r.y, r.width - labelWidth - 24f, r.height);

            EditorGUI.LabelField(labelRect, row.Label);

            // read floats
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
                // map to RGBA order by display name suffix
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
                // vector 2/3/4
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

        // -------------------- 32-bit: Single Row --------------------
        private void DrawSingleRow32(Rect r, SerializedProperty items32, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items32.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id) ?? new PropertyMetadata { Type = PropertyValueType.Int, Bitness = PropertyBitness.Bit32 };
            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 24f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            // key label
            EditorGUI.LabelField(keyRect, label);

            // remove
            if (GUI.Button(minusRect, "−", EditorStyles.miniButton))
            {
                items32.DeleteArrayElementAtIndex(index);
                items32.serializedObject.ApplyModifiedProperties();
                return;
            }

            // draw value
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
                        long ticks = DateTime.UnixEpoch.AddMinutes(rawProp.intValue).Ticks;
                        ticks = PropertyDrawerUtil.DrawDateTimeTicks(valRect, ticks, meta.Step ?? 60f);
                        int minutes = (int)Mathf.Clamp((float)TimeSpan.FromTicks(ticks).TotalMinutes, int.MinValue, int.MaxValue);
                        rawProp.intValue = minutes;
                        break;
                    }
                case PropertyValueType.TimeSpanShort:
                    {
                        long ticks = TimeSpan.FromMinutes(rawProp.intValue).Ticks;
                        ticks = PropertyDrawerUtil.DrawTimeSpanTicks(valRect, ticks, meta.Step ?? 5f, meta.Min, meta.Max);
                        int minutes = (int)Mathf.Clamp((float)TimeSpan.FromTicks(ticks).TotalMinutes, int.MinValue, int.MaxValue);
                        rawProp.intValue = minutes;
                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (32): {meta.Type}", MessageType.None);
                    break;
            }
        }

        // -------------------- 64-bit: Single Row --------------------
        private void DrawSingleRow64(Rect r, SerializedProperty items32, SerializedProperty items64, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items64.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id) ?? new PropertyMetadata { Type = PropertyValueType.Long, Bitness = PropertyBitness.Bit64 };
            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 24f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            // key label (readonly)
            EditorGUI.LabelField(keyRect, label);

            // remove
            if (GUI.Button(minusRect, "−", EditorStyles.miniButton))
            {
                items64.DeleteArrayElementAtIndex(index);
                items64.serializedObject.ApplyModifiedProperties();
                return;
            }

            long raw = rawProp.longValue;
            var u = new ValueUnion64 { raw = raw };

            switch (meta.Type)
            {
                case PropertyValueType.Float:
                    {
                        // Allowed if you choose to store 32f in 64 container; else omit
                        float v = u.asFloat;
                        v = PropertyDrawerUtil.DrawFloat(valRect, v, meta.Min, meta.Max, meta.Step);
                        u.asFloat = v;
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
                case PropertyValueType.Bool:
                    {
                        bool v = PropertyDrawerUtil.DrawBool(valRect, u.asBool);
                        u.asBool = v;
                        rawProp.longValue = u.raw;
                        break;
                    }
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
                case PropertyValueType.DateTime:
                    {
                        long ticks = u.raw;
                        DateTime dt = new DateTime(ticks, DateTimeKind.Utc);
                        string txt = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        string newTxt = EditorGUI.DelayedTextField(valRect, txt);
                        if (DateTime.TryParse(newTxt, out var parsed)) dt = parsed;

                        // Optional stepper (hour default)
                        float stepSec = meta.Step.HasValue && meta.Step.Value > 0f ? meta.Step.Value : 3600f;
                        var minusR = new Rect(valRect.xMax - 54, r.y, 25, r.height);
                        var plusR = new Rect(valRect.xMax - 27, r.y, 25, r.height);
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
                        string txt = EditorGUI.DelayedTextField(valRect, ts.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture));
                        if (EditorGUI.EndChangeCheck() && TimeSpan.TryParse(txt, out var parsed))
                            ts = parsed;

                        double stepSec = meta.Step.HasValue && meta.Step.Value > 0f ? meta.Step.Value : 60d;
                        var minusR = new Rect(valRect.xMax - 54, r.y, 25, r.height);
                        var plusR = new Rect(valRect.xMax - 27, r.y, 25, r.height);
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
                            int all = 0;
                            foreach (var v in Enum.GetValues(enumType)) all |= Convert.ToInt32(v);
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

        // -------------------- Helpers: sorting & write --------------------
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
    }
}
