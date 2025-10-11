using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor {
    [CustomPropertyDrawer(typeof(PropertySet32))]
    public class PropertySet32Drawer : PropertyDrawer
    {
        private struct Row
        {
            public string Label;        // Shown on the left (group name or single label)
            public string GroupKey;     // Non-null for grouped rows (group name)
            public List<int> Indices;   // indices into _items for this row
            public float Height;
            public bool IsGroup;
            public bool IsDuplicate;
            public PropertyGroupKind GroupKind; // Vector2, Vector3, Color, None
        }

        // ---------- Height ----------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropertyMetadataRegistry.EnsureBound();
            var resolver = PropertyMetadataRegistry.Resolver;
            if (resolver == null) return EditorGUIUtility.singleLineHeight * 2f;

            var itemsProp = property.FindPropertyRelative("_items");
            if (itemsProp == null || !itemsProp.isArray) return EditorGUIUtility.singleLineHeight * 2f;

            BuildRows(property, resolver, out var rows, out bool hasDuplicates);

            float total = EditorGUIUtility.singleLineHeight; // header
            if (hasDuplicates) total += EditorGUIUtility.singleLineHeight * 1.5f + 4f; // warning
            foreach (var r in rows) total += r.Height + 2f;
            return total + 6f;
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

            var itemsProp = property.FindPropertyRelative("_items");
            if (itemsProp == null || !itemsProp.isArray)
            {
                EditorGUI.HelpBox(position, "PropertySet32: internal list not found.", MessageType.Error);
                return;
            }

            var header = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawHeader(header, itemsProp, resolver);

            float y = header.yMax + 2f;

            BuildRows(property, resolver, out var rows, out bool hasDuplicates);
            if (hasDuplicates)
            {
                var warnRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 1.5f);
                EditorGUI.HelpBox(warnRect, "Duplicate parameters detected. Remove extras to avoid undefined behavior.", MessageType.Warning);
                y = warnRect.yMax + 4f;
            }

            foreach (var row in rows)
            {
                var r = new Rect(position.x, y, position.width, row.Height);

                if (row.IsDuplicate)
                {
                    var bg = new Color(1f, 0.85f, 0.85f, 0.35f);
                    EditorGUI.DrawRect(new Rect(r.x, r.y + 1f, r.width, r.height - 2f), bg);
                }

                if (row.IsGroup) DrawGroupRow(r, itemsProp, row, resolver);
                else DrawSingleRow(r, itemsProp, row.Indices[0], resolver);

                y += row.Height + 2f;
            }
        }

        // ---------- Build rows & detect duplicates ----------
        private void BuildRows(SerializedProperty property, IPropertyMetadataResolver resolver,
                               out List<Row> rows, out bool hasDuplicates)
        {
            var itemsProp = property.FindPropertyRelative("_items");
            rows = new List<Row>();
            hasDuplicates = false;

            int count = itemsProp.arraySize;
            if (count <= 0) return;

            // Duplicate detection
            var idToIndices = new Dictionary<int, List<int>>();
            for (int i = 0; i < count; i++)
            {
                int id = itemsProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                if (!idToIndices.TryGetValue(id, out var list)) { list = new List<int>(); idToIndices[id] = list; }
                list.Add(i);
            }
            hasDuplicates = idToIndices.Any(k => k.Value.Count > 1);

            // Grouping
            var consumed = new HashSet<int>();
            var groupBuckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < count; i++)
            {
                var elem = itemsProp.GetArrayElementAtIndex(i);
                int id = elem.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                if (meta == null) continue;

                // Only 32-bit items should even be in this set, but be defensive
                if (!meta.Bitness.HasFlag(PropertyBitness.Bit32)) continue;

                if (!string.IsNullOrEmpty(meta.GroupName))
                {
                    if (!groupBuckets.TryGetValue(meta.GroupName, out var list))
                    {
                        list = new List<int>();
                        groupBuckets[meta.GroupName] = list;
                    }
                    list.Add(i);
                }
            }

            // Build grouped rows
            foreach (var kvp in groupBuckets)
            {
                var indices = kvp.Value;
                // Pick group kind by the first member's metadata (all members should match)
                var firstMeta = resolver.Get(itemsProp.GetArrayElementAtIndex(indices[0]).FindPropertyRelative("id").intValue);
                var groupKind = firstMeta?.GroupKind ?? PropertyGroupKind.None;

                // Sorting strategy depends on kind (axis vs channels)
                List<int> ordered = groupKind switch
                {
                    PropertyGroupKind.Color => SortByColorChannels(itemsProp, indices, resolver, out _),
                    _ => SortByAxis(itemsProp, indices, resolver),
                };

                // Visual only supports 2..4 comps (Vector2, Vector3, Vector4/Color)
                int take = Mathf.Clamp(ordered.Count, 2, 4);
                if (take < 2) continue;

                var drawIndices = ordered.GetRange(0, take);
                foreach (var idx in drawIndices) consumed.Add(idx);

                bool isDup = drawIndices.Any(ix =>
                {
                    int id = itemsProp.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue;
                    return idToIndices[id].Count > 1;
                });

                rows.Add(new Row
                {
                    Label = kvp.Key,
                    GroupKey = kvp.Key,
                    Indices = drawIndices,
                    Height = EditorGUIUtility.singleLineHeight,
                    IsGroup = true,
                    IsDuplicate = isDup,
                    GroupKind = groupKind
                });
            }

            // Singles
            for (int i = 0; i < count; i++)
            {
                if (consumed.Contains(i)) continue;

                var elem = itemsProp.GetArrayElementAtIndex(i);
                int id = elem.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);

                // skip non-32 (defensive)
                if (meta != null && meta.Bitness != PropertyBitness.Bit32) continue;

                bool isDup = idToIndices.TryGetValue(id, out var list) && list.Count > 1;

                rows.Add(new Row
                {
                    Label = null,
                    GroupKey = null,
                    Indices = new List<int> { i },
                    Height = EditorGUIUtility.singleLineHeight,
                    IsGroup = false,
                    IsDuplicate = isDup,
                    GroupKind = PropertyGroupKind.None
                });
            }
        }

        // ---------- Header + Add (unused only, 32-bit only, group-aware) ----------
        private void DrawHeader(Rect r, SerializedProperty itemsProp, IPropertyMetadataResolver resolver)
        {
            var left = new Rect(r.x, r.y, r.width - 80f, r.height);
            var right = new Rect(r.xMax - 80f, r.y, 80f, r.height);

            EditorGUI.LabelField(left, ObjectNames.NicifyVariableName(itemsProp.displayName), EditorStyles.boldLabel);

            if (!GUI.Button(right, "+ Add", EditorStyles.miniButton)) return;

            // Existing IDs
            var existing = new HashSet<int>();
            for (int i = 0; i < itemsProp.arraySize; i++)
                existing.Add(itemsProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue);

            // Build enum defs (ONLY 32-bit)
            var names = resolver.GetAllNames();
            var values = resolver.GetAllValues();

            var groups = new Dictionary<string, (PropertyGroupKind kind, List<(int id, string disp)> items)>(StringComparer.OrdinalIgnoreCase);
            var singles = new List<(int id, string disp)>();

            for (int i = 0; i < values.Length; i++)
            {
                int id = values[i];
                var meta = resolver.Get(id);
                if (meta == null) continue;
                if (!meta.Bitness.HasFlag(PropertyBitness.Bit32)) continue;

                string disp = meta.DisplayName ?? names[i];

                if (!string.IsNullOrEmpty(meta.GroupName))
                {
                    if (!groups.TryGetValue(meta.GroupName, out var entry))
                    {
                        entry = (meta.GroupKind, new List<(int, string)>());
                        groups[meta.GroupName] = entry;
                    }
                    groups[meta.GroupName].items.Add((id, disp));
                }
                else
                {
                    if (!existing.Contains(id)) singles.Add((id, disp));
                }
            }

            var menu = new GenericMenu();

            // Groups: show only if any member is missing
            foreach (var kvp in groups.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var groupName = kvp.Key;
                var kind = kvp.Value.kind;
                var items = kvp.Value.items;

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
                            AddOne(itemsProp, id, 0);
                    itemsProp.serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() > 0 && singles.Count > 0) menu.AddSeparator("");

            // Singles: unused only
            foreach (var (id, disp) in singles.OrderBy(s => s.disp, StringComparer.OrdinalIgnoreCase))
            {
                menu.AddItem(new GUIContent(disp), false, () =>
                {
                    AddOne(itemsProp, id, 0);
                    itemsProp.serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No unused 32-bit parameters"));

            menu.ShowAsContext();
        }

        private static void AddOne(SerializedProperty itemsProp, int id, int raw = 0)
        {
            int newIndex = itemsProp.arraySize;
            itemsProp.arraySize++;
            var elem = itemsProp.GetArrayElementAtIndex(newIndex);
            elem.FindPropertyRelative("id").intValue = id;
            elem.FindPropertyRelative("rawValue").intValue = raw;
        }

        private static void RemoveAllByIds(SerializedProperty itemsProp, IEnumerable<int> ids)
        {
            var idSet = new HashSet<int>(ids);
            for (int i = itemsProp.arraySize - 1; i >= 0; i--)
            {
                var id = itemsProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                if (idSet.Contains(id))
                    itemsProp.DeleteArrayElementAtIndex(i);
            }
        }

        // ---------- Group row drawing (Vector2/3/Color) ----------
        private void DrawGroupRow(Rect r, SerializedProperty itemsProp, Row row, IPropertyMetadataResolver resolver)
        {
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);
            if (GUI.Button(minusRect, "−", EditorStyles.miniButton))
            {
                var ids = new List<int>(row.Indices.Count);
                foreach (int idx in row.Indices)
                {
                    var elem = itemsProp.GetArrayElementAtIndex(idx);
                    ids.Add(elem.FindPropertyRelative("id").intValue);
                }
                RemoveAllByIds(itemsProp, ids);
                itemsProp.serializedObject.ApplyModifiedProperties();
                return;
            }

            float labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(r.x, r.y, labelWidth, r.height);
            var fieldRect = new Rect(r.x + labelWidth, r.y, r.width - labelWidth - 24f, r.height);

            EditorGUI.LabelField(labelRect, row.Label);

            // Read float components
            int comp = row.Indices.Count;
            var values = new float[comp];
            for (int i = 0; i < comp; i++)
            {
                var elem = itemsProp.GetArrayElementAtIndex(row.Indices[i]);
                int raw = elem.FindPropertyRelative("rawValue").intValue;
                var u = new ValueUnion32 { raw = raw };
                values[i] = u.asFloat;
            }

            if (row.GroupKind == PropertyGroupKind.Color && comp >= 3)
            {
                // sort to R,G,B,A
                var ordered = SortByColorChannels(itemsProp, row.Indices, resolver, out int use);
                var rgba = new float[4] { 1, 1, 1, 1 };
                for (int i = 0; i < use; i++)
                {
                    int srcIdx = ordered[i];
                    var elem = itemsProp.GetArrayElementAtIndex(srcIdx);
                    int raw = elem.FindPropertyRelative("rawValue").intValue;
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
                    if (use > 0) WriteFloat(itemsProp, ordered[0], col.r);
                    if (use > 1) WriteFloat(itemsProp, ordered[1], col.g);
                    if (use > 2) WriteFloat(itemsProp, ordered[2], col.b);
                    if (use > 3) WriteFloat(itemsProp, ordered[3], col.a);
                    itemsProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                // Vector2/3/4 drawing
                EditorGUI.BeginChangeCheck();
                switch (comp)
                {
                    case 2:
                        {
                            var v = new Vector2(values[0], values[1]);
                            v = EditorGUI.Vector2Field(fieldRect, GUIContent.none, v);
                            values[0] = v.x; values[1] = v.y;
                            break;
                        }
                    case 3:
                        {
                            var v = new Vector3(values[0], values[1], values[2]);
                            v = EditorGUI.Vector3Field(fieldRect, GUIContent.none, v);
                            values[0] = v.x; values[1] = v.y; values[2] = v.z;
                            break;
                        }
                    default:
                        {
                            var v = new Vector4(values[0], values[1], values[2], values[3]);
                            v = EditorGUI.Vector4Field(fieldRect, GUIContent.none, v);
                            values[0] = v.x; values[1] = v.y; values[2] = v.z; values[3] = v.w;
                            break;
                        }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < comp; i++) WriteFloat(itemsProp, row.Indices[i], values[i]);
                    itemsProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // ---------- Single row (label key, value field, remove) ----------
        private void DrawSingleRow(Rect r, SerializedProperty itemsProp, int index, IPropertyMetadataResolver resolver)
        {
            var elem = itemsProp.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id) ?? new PropertyMetadata { Type = PropertyValueType.Int, Bitness = PropertyBitness.Bit32 };
            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 24f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            // Key is label (not editable)
            EditorGUI.LabelField(keyRect, label);

            // Remove button
            if (GUI.Button(minusRect, "−", EditorStyles.miniButton))
            {
                RemoveAllByIds(itemsProp, new[] { id });
                itemsProp.serializedObject.ApplyModifiedProperties();
                return;
            }

            // Value
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

        // ---------- Sorting helpers ----------
        private List<int> SortByAxis(SerializedProperty itemsProp, List<int> indices, IPropertyMetadataResolver resolver)
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
                var elem = itemsProp.GetArrayElementAtIndex(i);
                int id = elem.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                var disp = meta?.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id);
                return Score(disp);
            }).ToList();
        }

        private List<int> SortByColorChannels(SerializedProperty itemsProp, List<int> indices, IPropertyMetadataResolver resolver, out int use)
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
                var elem = itemsProp.GetArrayElementAtIndex(i);
                int id = elem.FindPropertyRelative("id").intValue;
                var meta = resolver.Get(id);
                var disp = meta?.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id);
                return Score(disp);
            }).ToList();

            use = Mathf.Min(4, ordered.Count);
            return ordered;
        }

        // ---------- Small util ----------
        private static void WriteFloat(SerializedProperty itemsProp, int itemIndex, float value)
        {
            var elem = itemsProp.GetArrayElementAtIndex(itemIndex);
            var u = new ValueUnion32 { asFloat = value };
            elem.FindPropertyRelative("rawValue").intValue = u.raw;
        }
    }
}
