
using DynamicProperty.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    [CustomPropertyDrawer(typeof(PropertySet))]
    public class PropertySetDrawer : PropertyDrawer
    {
        private enum Backing { Bit32, Bit64 }

        private enum RowState { Normal, Unknown, WrongStorage, InvalidGroup, PartialGroup }

        private struct Row
        {
            public Backing Bin;            // which internal list this row belongs to
            public bool IsGroup;           // grouped 32-bit float row (Vector/Color)
            public PropertyGroupKind Kind; // None / Vector2 / Vector3 / Color
            public string Label;           // group label or single display name
            public List<int> Indices;      // indices into the corresponding list (_items32/_items64)
            public bool IsDuplicate;       // highlight duplicate
            public RowState State;
            
            public string Category;
            public int? Order;
            public int SchemaOrder;

            public float Height;
        }

        private struct ValidationSummary
        {
            public bool HasDuplicates;
            public bool HasUnknownProperties;
            public bool HasWrongStorage;
            public bool HasInvalidGroups;
            public bool HasPartialGroups;

            public bool HasErrors =>
                HasDuplicates ||
                HasWrongStorage ||
                HasInvalidGroups ||
                HasPartialGroups;

            public bool HasWarnings =>
                HasUnknownProperties;

            public bool HasAny =>
                HasErrors ||
                HasWarnings;
        }

        // ---------- Routing by metadata ----------
        private static bool Is64Type(PropertyMetadata meta)
        {
            if (meta == null)
                return false;

            switch (meta.Type)
            {
                case PropertyValueType.Long:
                case PropertyValueType.Double:
                case PropertyValueType.DateTime:
                case PropertyValueType.TimeSpan:
                    return true;

                case PropertyValueType.Enum:
                    return meta.EnumType != null && EnumBitUtility.Uses64BitStorage(meta.EnumType);

                default:
                    return false;
            }
        }

        // ---------- Height ----------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!TryResolveMetadata(out var resolver, out _))
            {
                return EditorGUIUtility.singleLineHeight * 2f + 8f;
            }

            var (items32, items64) = GetLists(property);

            if (items32 == null || items64 == null)
            {
                return EditorGUIUtility.singleLineHeight * 2f + 8f;
            }

            BuildRowsUnified(property, resolver, out var rows, out var validation);

            float height = EditorGUIUtility.singleLineHeight + 4f;

            if (validation.HasAny)
                height += EditorGUIUtility.singleLineHeight * 2f + 4f;

            string currentCategory = null;

            foreach (var row in rows)
            {
                if (!string.Equals(currentCategory, row.Category, StringComparison.OrdinalIgnoreCase))
                {
                    currentCategory = row.Category;

                    height += GetCategoryHeaderHeight();
                }

                height += row.Height + 2f;
            }

            return height + 6f;
        }

        // ---------- GUI ----------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryResolveMetadata(out var resolver, out var error))
            {
                EditorGUI.HelpBox(position, error, MessageType.Error);

                return;
            }

            var (items32, items64) = GetLists(property);

            if (items32 == null || items64 == null)
            {
                EditorGUI.HelpBox(
                    position,
                    "PropertySet internal lists were not found.",
                    MessageType.Error);

                return;
            }

            float y = position.y;

            // Title + single Add
            var titleRect = new Rect(position.x, y, position.width - 90f, EditorGUIUtility.singleLineHeight);
            var addRect = new Rect(position.x + position.width - 90f, y, 90f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(titleRect, ObjectNames.NicifyVariableName(property.displayName), EditorStyles.boldLabel);
            if (GUI.Button(addRect, "+ Add", EditorStyles.miniButton))
            {
                ShowAddPicker(addRect, property, items32, items64, resolver);
            }
            y += titleRect.height + 4f;

            // Rows
            BuildRowsUnified(property, resolver, out var rows, out var validation);

            if (validation.HasAny)
            {
                var validationRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 2f);

                EditorGUI.HelpBox(
                    validationRect,
                    GetValidationSummaryMessage(validation),
                    validation.HasErrors
                        ? MessageType.Error
                        : MessageType.Warning);

                y += validationRect.height + 4f;
            }

            string currentCategory = null;

            foreach (var row in rows)
            {
                if (!string.Equals(currentCategory, row.Category, StringComparison.OrdinalIgnoreCase))
                {
                    currentCategory = row.Category;

                    y += DrawCategoryHeader(position, y, currentCategory);
                }

                var r = new Rect(position.x, y, position.width, row.Height);

                if (row.IsDuplicate ||
                    row.State == RowState.WrongStorage ||
                    row.State == RowState.InvalidGroup ||
                    row.State == RowState.PartialGroup)
                {
                    EditorGUI.DrawRect(
                        new Rect(r.x, r.y + 1f, r.width, r.height - 2f),
                        new Color(1f, 0.3f, 0.3f, 0.18f));
                }
                else if (row.State == RowState.Unknown)
                {
                    EditorGUI.DrawRect(
                        new Rect(r.x, r.y + 1f, r.width, r.height - 2f),
                        new Color(1f, 0.75f, 0.15f, 0.22f));
                }

                bool structureChanged;

                if (row.IsGroup)
                {
                    structureChanged = DrawGroupRow32(r, property, row, items32, resolver);
                }
                else if (row.Bin == Backing.Bit32)
                {
                    structureChanged = DrawSingleRow32(r, property, items32, row.Indices[0], resolver);
                }
                else
                {
                    structureChanged = DrawSingleRow64(r, property, items64, row.Indices[0], resolver);
                }

                if (structureChanged)
                    return;

                y += row.Height + 2f;
            }
        }

        // ---------- Build unified rows (group 32-bit float groups; singles: 32 then 64) ----------
        private void BuildRowsUnified(
            SerializedProperty property,
            IPropertyMetadataResolver resolver,
            out List<Row> rows,
            out ValidationSummary validation)
        {
            var (items32, items64) = GetLists(property);
            rows = new List<Row>();
            validation = new ValidationSummary();

            var schemaValues = resolver.GetAllValues();
            var schemaOrder = new Dictionary<int, int>();
            for (int i = 0; i < schemaValues.Length; i++)
                schemaOrder[schemaValues[i]] = i;

            var groupDefinitions = PropertyGroupValidator.Build(resolver);

            validation.HasInvalidGroups = groupDefinitions.Values.Any(definition => !definition.IsValid);

            bool dup32 = HasDuplicates(items32, out var map32);
            bool dup64 = HasDuplicates(items64, out var map64);
            var ids32 = CollectIds(items32);
            var ids64 = CollectIds(items64);
            bool cross = ids32.Overlaps(ids64);
            validation.HasDuplicates = dup32 || dup64 || cross;

            var partialGroupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in groupDefinitions.Values)
            {
                if (!definition.IsValid)
                    continue;

                int presentCount = definition.MemberIds.Count(id => ids32.Contains(id) || ids64.Contains(id));

                if (presentCount == 0)
                {
                    // Entire group is absent.
                    // This is a valid sparse state.
                    continue;
                }

                if (presentCount == definition.MemberIds.Count)
                {
                    // All logical members exist somewhere.
                    // Wrong-storage validation is handled separately.
                    continue;
                }

                partialGroupNames.Add(definition.Name);

                validation.HasPartialGroups = true;
            }

            var consumed32 = new HashSet<int>();
            var groupBuckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            // 32-bit group buckets (float + group name)
            for (int i = 0; i < items32.arraySize; i++)
            {
                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;

                var meta = resolver.Get(id);

                if (meta == null || string.IsNullOrWhiteSpace(meta.GroupName))
                    continue;

                string groupName = meta.GroupName.Trim();

                if (!groupDefinitions.TryGetValue(groupName, out var definition))
                    continue;

                // Invalid schema groups must not be interpreted
                // as aggregates.
                if (!definition.IsValid)
                    continue;

                if (!groupBuckets.TryGetValue(groupName, out var list))
                {
                    list = new List<int>();
                    groupBuckets[groupName] = list;
                }

                list.Add(i);
            }

            // Group rows
            foreach (var kvp in groupBuckets)
            {
                var indices = kvp.Value;

                if (indices.Count < 2)
                    continue;

                if (!groupDefinitions.TryGetValue(kvp.Key, out var definition))
                    continue;

                if (!definition.IsValid)
                    continue;

                var kind = definition.Kind;

                var indexById = new Dictionary<int, int>();

                foreach (int index in indices)
                {
                    int id = items32
                            .GetArrayElementAtIndex(index)
                            .FindPropertyRelative("id")
                            .intValue;

                    // Duplicate handling belongs to the duplicate validator.
                    // For aggregate mapping we keep the first serialized entry.
                    if (!indexById.ContainsKey(id))
                        indexById[id] = index;
                }

                bool isComplete = definition.MemberIds.All(id => indexById.ContainsKey(id));

                if (!isComplete)
                {
                    // Partial groups must not be interpreted
                    // as aggregate values.
                    //
                    // Their members will remain unconsumed
                    // and will be rendered as single rows below.
                    continue;
                }

                var draw = definition.MemberIds
                        .Select(id => indexById[id])
                        .ToList();

                foreach (var idx in draw) consumed32.Add(idx);

                bool d = draw.Any(ix =>
                {
                    int id = items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue;
                    return (map32.TryGetValue(id, out var c) && c > 1) || ids64.Contains(id);
                });

                string category = null;
                int? order = null;
                int groupSchemaOrder = int.MaxValue;

                var orderedGroupItems = draw.Select(index =>
                    {
                        int id = items32.GetArrayElementAtIndex(index).FindPropertyRelative("id").intValue;

                        int orderInSchema = schemaOrder.TryGetValue(id, out var schemaIndex) ? schemaIndex : int.MaxValue;

                        return new
                        {
                            Id = id,
                            Meta = resolver.Get(id),
                            SchemaOrder = orderInSchema
                        };
                    })
                    .OrderBy(item => item.SchemaOrder)
                    .ToList();

                foreach (var item in orderedGroupItems)
                {
                    groupSchemaOrder = Math.Min(groupSchemaOrder, item.SchemaOrder);

                    if (item.Meta == null)
                        continue;

                    if (category == null && !string.IsNullOrWhiteSpace(item.Meta.Category))
                        category = item.Meta.Category;

                    if (!order.HasValue && item.Meta.Order.HasValue)
                        order = item.Meta.Order;
                }

                category = NormalizeCategory(category);

                rows.Add(new Row
                {
                    Bin = Backing.Bit32,
                    IsGroup = true,
                    Kind = kind,
                    Label = kvp.Key,
                    Indices = draw,
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight,
                    State = RowState.Normal,
                    Category = category,
                    Order = order,
                    SchemaOrder = groupSchemaOrder
                });
            }

            // 32-bit singles (non-grouped or unmatched)
            for (int i = 0; i < items32.arraySize; i++)
            {
                if (consumed32.Contains(i)) continue;

                int id = items32.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                bool d = (map32.TryGetValue(id, out var c) && c > 1) || ids64.Contains(id);

                var meta = resolver.Get(id);

                string category = NormalizeCategory(meta?.Category);

                int? order = meta?.Order;

                int itemSchemaOrder = schemaOrder.TryGetValue(id, out var schemaIndex) ? schemaIndex : int.MaxValue;

                RowState state;

                if (meta == null)
                {
                    state = RowState.Unknown;
                    validation.HasUnknownProperties = true;
                }
                else if (IsMemberOfInvalidGroup(meta, groupDefinitions))
                {
                    state = RowState.InvalidGroup;
                }
                else if (Is64Type(meta))
                {
                    state = RowState.WrongStorage;
                    validation.HasWrongStorage = true;
                }
                else if (IsMemberOfPartialGroup(meta, partialGroupNames))
                {
                    state = RowState.PartialGroup;
                }
                else
                {
                    state = RowState.Normal;
                }

                rows.Add(new Row
                {
                    Bin = Backing.Bit32,
                    IsGroup = false,
                    Kind = PropertyGroupKind.None,
                    Label = null,
                    Indices = new List<int> { i },
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight,
                    State = state,
                    Category = category,
                    Order = order,
                    SchemaOrder = itemSchemaOrder
                });
            }

            // 64-bit singles
            for (int i = 0; i < items64.arraySize; i++)
            {
                int id = items64.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue;
                bool d = (map64.TryGetValue(id, out var c) && c > 1) || ids32.Contains(id);

                var meta = resolver.Get(id);

                string category = NormalizeCategory(meta?.Category);

                int? order = meta?.Order;

                int itemSchemaOrder = schemaOrder.TryGetValue(id, out var schemaIndex) ? schemaIndex : int.MaxValue;

                RowState state;

                if (meta == null)
                {
                    state = RowState.Unknown;
                    validation.HasUnknownProperties = true;
                }
                else if (IsMemberOfInvalidGroup(meta, groupDefinitions))
                {
                    state = RowState.InvalidGroup;
                }
                else if (!Is64Type(meta))
                {
                    state = RowState.WrongStorage;
                    validation.HasWrongStorage = true;
                }
                else if (IsMemberOfPartialGroup(meta, partialGroupNames))
                {
                    state = RowState.PartialGroup;
                }
                else
                {
                    state = RowState.Normal;
                }

                rows.Add(new Row
                {
                    Bin = Backing.Bit64,
                    IsGroup = false,
                    Kind = PropertyGroupKind.None,
                    Label = null,
                    Indices = new List<int> { i },
                    IsDuplicate = d,
                    Height = EditorGUIUtility.singleLineHeight, 
                    State = state,
                    Category = category,
                    Order = order,
                    SchemaOrder = itemSchemaOrder
                });
            }

            var categoryOrder = rows
                .GroupBy(row => row.Category, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Min(row => row.SchemaOrder),
                    StringComparer.OrdinalIgnoreCase);
            
            rows = rows
                .OrderBy(row => categoryOrder[row.Category])
                .ThenBy(row => row.Order.HasValue ? 0 : 1)
                .ThenBy(row => row.Order ?? int.MaxValue)
                .ThenBy(row => row.SchemaOrder)
                .ToList();
        }

        // ---------- Add picker ----------

        private void ShowAddPicker(Rect activatorRect, SerializedProperty propertySet, SerializedProperty items32, SerializedProperty items64, IPropertyMetadataResolver resolver)
        {
            var existing = CollectIds(items32);
            existing.UnionWith(CollectIds(items64));

            var names = resolver.GetAllNames();
            var values = resolver.GetAllValues();

            var groupDefinitions = PropertyGroupValidator.Build(resolver);

            var groups32 = new Dictionary<string, (PropertyGroupKind kind, List<(int id, string displayName)> items)>(StringComparer.OrdinalIgnoreCase);

            var singles = new List<(int id, string displayName, PropertyMetadata meta)>();

            for (int i = 0; i < values.Length; i++)
            {
                int id = values[i];

                if (existing.Contains(id))
                    continue;

                var meta = resolver.Get(id);

                if (meta == null)
                    continue;

                if (meta.HiddenInEditor || id == 0)
                    continue;

                string displayName = meta.DisplayName ?? names[i];

                if (!string.IsNullOrWhiteSpace(meta.GroupName))
                {
                    string groupName = meta.GroupName.Trim();

                    if (!groupDefinitions.TryGetValue(groupName, out var definition))
                        continue;

                    // Invalid groups cannot be added from the picker.
                    if (!definition.IsValid)
                        continue;

                    if (!groups32.TryGetValue(groupName, out var group))
                    {
                        group = (definition.Kind, new List<(int, string)>());
                    }

                    group.items.Add((id, displayName));

                    groups32[groupName] = group;

                    continue;
                }

                singles.Add((id, displayName, meta));
            }

            var pickerItems = new List<PropertyPickerPopup.Item>();

            AddGroupPickerItems(pickerItems, groups32, groupDefinitions, propertySet, items32, resolver);

            AddSinglePickerItems(pickerItems, singles, propertySet, items32, items64);

            PopupWindow.Show(activatorRect, new PropertyPickerPopup(pickerItems));
        }

        private void AddGroupPickerItems(
            List<PropertyPickerPopup.Item> pickerItems,
            Dictionary<string, (PropertyGroupKind kind, List<(int id, string displayName)> items)> groups,
            IReadOnlyDictionary<string, PropertyGroupDefinition> definitions,
            SerializedProperty propertySet,
            SerializedProperty items32,
            IPropertyMetadataResolver resolver)
        {
            foreach (var kvp in groups)
            {
                string groupName = kvp.Key;
                var group = kvp.Value;

                if (!definitions.TryGetValue(groupName, out var definition))
                    continue;

                if (!definition.IsValid)
                    continue;

                string label = groupName;

                switch (group.kind)
                {
                    case PropertyGroupKind.Color:
                        label += " (Color)";
                        break;

                    case PropertyGroupKind.Vector2:
                        label += " (Vector2)";
                        break;

                    case PropertyGroupKind.Vector3:
                        label += " (Vector3)";
                        break;

                    case PropertyGroupKind.Vector4:
                        label += " (Vector4)";
                        break;
                }

                var itemsById = group.items.ToDictionary(item => item.id);

                var capturedItems = definition.MemberIds
                        .Where(itemsById.ContainsKey)
                        .Select(id => itemsById[id])
                        .ToArray();

                string searchText = label + " " + string.Join(" ", capturedItems.Select(x => x.displayName));

                string category = null;
                string tooltip = null;
                int? order = null;

                foreach (int memberId in definition.MemberIds)
                {
                    var meta = resolver.Get(memberId);

                    if (meta == null)
                        continue;

                    if (category == null && !string.IsNullOrWhiteSpace(meta.Category))
                        category = meta.Category;

                    if (tooltip == null && !string.IsNullOrWhiteSpace(meta.Tooltip))
                        tooltip = meta.Tooltip;

                    if (!order.HasValue && meta.Order.HasValue)
                        order = meta.Order;

                    if (category != null && tooltip != null && order.HasValue)
                        break;
                }

                pickerItems.Add(new PropertyPickerPopup.Item(label, searchText, category, order, tooltip, () =>
                    {
                        foreach (var item in capturedItems)
                        {
                            var meta = resolver.Get(item.id);

                            Add32(items32, item.id, GetInitialRaw32(meta));
                        }

                        MarkStructureChanged(propertySet);

                        propertySet.serializedObject.ApplyModifiedProperties();
                    }));
            }
        }

        private void AddSinglePickerItems(
            List<PropertyPickerPopup.Item> pickerItems,
            List<(int id, string displayName, PropertyMetadata meta)> singles,
            SerializedProperty propertySet,
            SerializedProperty items32,
            SerializedProperty items64)
        {
            foreach (var entry in singles)
            {
                int id = entry.id;
                string displayName = entry.displayName;
                PropertyMetadata meta = entry.meta;

                bool is64 = Is64Type(meta);

                string label = displayName;

                pickerItems.Add(
                    new PropertyPickerPopup.Item(label, displayName, meta.Category, meta.Order, meta.Tooltip, () =>
                        {
                            if (is64)
                                Add64(items64, id, GetInitialRaw64(meta));
                            else
                                Add32(items32, id, GetInitialRaw32(meta));

                            MarkStructureChanged(propertySet);

                            propertySet.serializedObject.ApplyModifiedProperties();
                        }));
            }
        }

        // ---------- 32-bit grouped row ----------
        private bool DrawGroupRow32(Rect r, SerializedProperty propertySet, Row row, SerializedProperty items32, IPropertyMetadataResolver resolver)
        {
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);
            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                var ids = row.Indices.Select(ix => items32.GetArrayElementAtIndex(ix).FindPropertyRelative("id").intValue);
                RemoveByIds(items32, ids);

                MarkStructureChanged(propertySet);
                propertySet.serializedObject.ApplyModifiedProperties();

                return true;
            }

            float third = r.width / 3f;
            var labelRect = new Rect(r.x, r.y, third, r.height);
            var fieldRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);

            string tooltip = null;

            foreach (int index in row.Indices)
            {
                var element = items32.GetArrayElementAtIndex(index);

                int id = element .FindPropertyRelative("id") .intValue;

                var meta = resolver.Get(id);

                if (meta == null || string.IsNullOrWhiteSpace(meta.Tooltip))
                    continue;

                tooltip = meta.Tooltip;
                break;
            }

            EditorGUI.LabelField(labelRect, new GUIContent(row.Label, tooltip));

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
                var ordered = row.Indices;

                int use = Mathf.Min(4, ordered.Count);
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

            return false;
        }

        // ---------- 32-bit single ----------
        private bool DrawSingleRow32(Rect r, SerializedProperty propertySet, SerializedProperty items32, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items32.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id);

            if (meta == null)
                return DrawInvalidRow(r, propertySet, items32, index, $"Unknown Property (ID {id})", $"Raw: {rawProp.intValue}", MessageType.Warning);

            string label = meta.DisplayName ?? Enum.GetName(resolver.BoundEnumType, id) ?? $"ID {id}";

            if (Is64Type(meta))
                return DrawInvalidRow(r, propertySet, items32, index, label, $"Expected 64-bit storage. Raw: {rawProp.intValue}", MessageType.Error);

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            EditorGUI.LabelField(keyRect, new GUIContent(label, meta.Tooltip));

            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                items32.DeleteArrayElementAtIndex(index);

                MarkStructureChanged(propertySet);
                propertySet.serializedObject.ApplyModifiedProperties();

                return true;
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
                        if (meta.EnumType == null)
                        {
                            EditorGUI.HelpBox(valRect, "Enum type not defined!", MessageType.Warning);

                            break;
                        }

                        if (EnumBitUtility.Uses64BitStorage(meta.EnumType))
                        {
                            EditorGUI.HelpBox(valRect, "Enum requires 64-bit storage.", MessageType.Error);

                            break;
                        }

                        rawProp.intValue = PropertyDrawerUtil.DrawEnum32(valRect, rawProp.intValue, meta.EnumType,
                            value =>
                            {
                                rawProp.intValue = value;

                                rawProp.serializedObject
                                    .ApplyModifiedProperties();
                            });

                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (32): {meta.Type}", MessageType.None);
                    break;
            }

            return false;
        }

        // ---------- 64-bit single ----------
        private bool DrawSingleRow64(Rect r, SerializedProperty propertySet, SerializedProperty items64, int index, IPropertyMetadataResolver resolver)
        {
            var elem = items64.GetArrayElementAtIndex(index);
            var idProp = elem.FindPropertyRelative("id");
            var rawProp = elem.FindPropertyRelative("rawValue");

            int id = idProp.intValue;
            var meta = resolver.Get(id);

            if (meta == null)
                return DrawInvalidRow(r, propertySet, items64, index, $"Unknown Property (ID {id})", $"Raw: {rawProp.longValue}", MessageType.Warning);

            string label =
                meta.DisplayName ??
                Enum.GetName(resolver.BoundEnumType, id) ??
                $"ID {id}";

            if (!Is64Type(meta))
                return DrawInvalidRow(r, propertySet, items64, index, label, $"Expected 32-bit storage. Raw: {rawProp.longValue}", MessageType.Error);

            float third = r.width / 3f;
            var keyRect = new Rect(r.x, r.y, third, r.height);
            var valRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);
            var minusRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            EditorGUI.LabelField(keyRect, new GUIContent(label, meta.Tooltip));

            if (GUI.Button(minusRect, "x", EditorStyles.miniButton))
            {
                items64.DeleteArrayElementAtIndex(index);

                MarkStructureChanged(propertySet);
                propertySet.serializedObject.ApplyModifiedProperties();

                return true;
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
                        if (meta.EnumType == null)
                        {
                            EditorGUI.HelpBox(valRect, "Enum type not defined!", MessageType.Warning);

                            break;
                        }

                        if (!EnumBitUtility.Uses64BitStorage(meta.EnumType))
                        {
                            EditorGUI.HelpBox(valRect, "Enum requires 32-bit storage.", MessageType.Error);

                            break;
                        }

                        rawProp.longValue = PropertyDrawerUtil.DrawEnum64(valRect, rawProp.longValue, meta.EnumType,
                            value =>
                            {
                                rawProp.longValue = value;

                                rawProp.serializedObject
                                    .ApplyModifiedProperties();
                            });

                        break;
                    }
                default:
                    EditorGUI.HelpBox(valRect, $"Type not handled (64): {meta.Type}", MessageType.None);
                    break;
            }

            return false;
        }

        // ---------- Helpers ----------

        private static bool DrawInvalidRow(
            Rect r,
            SerializedProperty propertySet,
            SerializedProperty list,
            int index,
            string label,
            string message,
            MessageType severity)
        {
            float third = r.width / 3f;

            var iconRect = new Rect(r.x + 2f, r.y, 18f, r.height);

            var labelRect = new Rect(r.x + 22f, r.y, third - 22f, r.height);

            var messageRect = new Rect(r.x + third + 5f, r.y, 2f * third - 5f - 27f, r.height);

            var removeRect = new Rect(r.xMax - 22f, r.y, 20f, r.height);

            GUIContent icon =
                severity == MessageType.Error
                    ? EditorGUIUtility.IconContent("console.erroricon.sml")
                    : EditorGUIUtility.IconContent("console.warnicon.sml");

            EditorGUI.LabelField(iconRect, icon);

            EditorGUI.LabelField(labelRect, label);

            EditorGUI.LabelField(messageRect, message, EditorStyles.miniLabel);

            if (!GUI.Button(removeRect, "x", EditorStyles.miniButton))
                return false;

            list.DeleteArrayElementAtIndex(index);

            MarkStructureChanged(propertySet);

            propertySet.serializedObject
                .ApplyModifiedProperties();

            return true;
        }

        private static void MarkStructureChanged(SerializedProperty propertySet)
        {
            var version = propertySet.FindPropertyRelative("_structureVersion");

            version.intValue++;
        }

        private bool TryResolveMetadata(out IPropertyMetadataResolver resolver, out string error)
        {
            resolver = null;
            error = null;

            if (fieldInfo == null)
            {
                error = "Unable to resolve PropertySet field information.";

                return false;
            }

            var schema = fieldInfo.GetCustomAttribute<PropertySchemaAttribute>(true);

            if (schema == null)
            {
                error = "PropertySet requires [PropertySchema(typeof(...))].";

                return false;
            }

            return PropertyMetadataRegistry.TryGetResolver(schema.SchemaType, out resolver, out error);
        }

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

        private static void WriteFloat(SerializedProperty items32, int itemIndex, float value)
        {
            var e = items32.GetArrayElementAtIndex(itemIndex);
            var u = new ValueUnion32 { asFloat = value };
            e.FindPropertyRelative("rawValue").intValue = u.raw;
        }


        private static int GetInitialRaw32(PropertyMetadata meta)
        {
            if (meta == null || !meta.HasInitialValue)
                return 0;

            switch (meta.Type)
            {
                case PropertyValueType.Int:
                    return (int)meta.InitialValue;

                case PropertyValueType.Bool:
                    return (bool)meta.InitialValue ? 1 : 0;

                case PropertyValueType.Float:
                    return new ValueUnion32
                    {
                        asFloat = (float)meta.InitialValue
                    }.raw;

                case PropertyValueType.Enum:
                    return EnumBitUtility.ToRaw32(meta.EnumType, (Enum)meta.InitialValue);

                default:
                    return 0;
            }
        }

        private static long GetInitialRaw64(PropertyMetadata meta)
        {
            if (meta == null || !meta.HasInitialValue)
                return 0L;

            switch (meta.Type)
            {
                case PropertyValueType.Long:
                    return (long)meta.InitialValue;

                case PropertyValueType.Double:
                    return new ValueUnion64
                    {
                        asDouble = (double)meta.InitialValue
                    }.raw;

                case PropertyValueType.Enum:
                    return EnumBitUtility.ToRaw64(
                        meta.EnumType,
                        (Enum)meta.InitialValue);

                default:
                    return 0L;
            }
        }

        private static string GetValidationSummaryMessage(ValidationSummary validation)
        {
            var problems = new List<string>();

            if (validation.HasDuplicates)
                problems.Add("duplicate/conflicting IDs");

            if (validation.HasWrongStorage)
                problems.Add("wrong-storage entries");

            if (validation.HasInvalidGroups)
                problems.Add("invalid group definitions");

            if (validation.HasUnknownProperties)
                problems.Add("unknown properties");

            if (validation.HasPartialGroups)
                problems.Add("incomplete aggregate groups");

            return
                "PropertySet contains " +
                string.Join(", ", problems) +
                ". Remove or migrate the highlighted entries.";
        }

        private static string NormalizeCategory(string category)
        {
            return string.IsNullOrWhiteSpace(category)
                ? "General"
                : category.Trim();
        }

        private static float GetCategoryHeaderHeight()
        {
            const float topSpacing = 6f;
            const float separatorHeight = 1f;
            const float bottomSpacing = 4f;

            return
                topSpacing +
                EditorGUIUtility.singleLineHeight +
                separatorHeight +
                bottomSpacing;
        }

        private static float DrawCategoryHeader(Rect position, float y, string category)
        {
            const float topSpacing = 6f;

            float lineHeight = EditorGUIUtility.singleLineHeight;

            float labelY = y + topSpacing;

            var labelRect = new Rect(position.x + 2f, labelY, position.width - 4f, lineHeight);

            EditorGUI.LabelField(labelRect, category, EditorStyles.boldLabel);

            var separatorRect = new Rect(position.x, labelRect.yMax, position.width, 1f);

            EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 0.35f));

            return GetCategoryHeaderHeight();
        }

        private static bool IsMemberOfInvalidGroup(PropertyMetadata meta, IReadOnlyDictionary<string, PropertyGroupDefinition> definitions)
        {
            if (meta == null || string.IsNullOrWhiteSpace(meta.GroupName))
                return false;

            string groupName = meta.GroupName.Trim();

            return definitions.TryGetValue(groupName, out var definition) && !definition.IsValid;
        }

        private static bool IsMemberOfPartialGroup(PropertyMetadata meta, HashSet<string> partialGroupNames)
        {
            if (meta == null || string.IsNullOrWhiteSpace(meta.GroupName))
                return false;

            return partialGroupNames.Contains(meta.GroupName.Trim());
        }
    }
}