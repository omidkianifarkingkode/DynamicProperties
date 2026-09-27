using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    internal sealed class PropertyPickerPopup : PopupWindowContent
    {
        internal readonly struct Item
        {
            public string Label { get; }
            public string SearchText { get; }

            public string Category { get; }
            public int? Order { get; }
            public string Tooltip { get; }

            public Action OnSelected { get; }

            public Item(
                string label,
                string searchText,
                string category,
                int? order,
                string tooltip,
                Action onSelected)
            {
                Label = label;
                SearchText = searchText;

                Category = string.IsNullOrWhiteSpace(category)
                    ? "General"
                    : category.Trim();

                Order = order;
                Tooltip = tooltip ?? string.Empty;

                OnSelected = onSelected;
            }
        }

        private const float Width = 360f;
        private const float MaxHeight = 420f;

        private readonly IReadOnlyList<Item> _items;

        private string _search = string.Empty;
        private Vector2 _scroll;
        private bool _focusSearch = true;

        public PropertyPickerPopup(
            IReadOnlyList<Item> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _items = items
                .OrderBy(
                    item => item.Category,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    item => item.Order.HasValue ? 0 : 1)
                .ThenBy(
                    item => item.Order ?? int.MaxValue)
                .ThenBy(
                    item => item.Label,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public override Vector2 GetWindowSize()
        {
            float rowHeight =
                EditorGUIUtility.singleLineHeight + 4f;

            int categoryCount =
                _items
                    .Select(item => item.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

            float contentHeight =
                38f +
                (_items.Count + categoryCount) * rowHeight;

            return new Vector2(
                Width,
                Mathf.Min(MaxHeight, contentHeight));
        }

        public override void OnGUI(Rect rect)
        {
            HandleKeyboard();

            DrawSearch();

            EditorGUILayout.Space(2f);

            _scroll =
                EditorGUILayout.BeginScrollView(_scroll);

            int visibleCount = 0;
            string currentCategory = null;

            foreach (var item in _items)
            {
                if (!MatchesSearch(item))
                    continue;

                visibleCount++;

                if (!string.Equals(
                        currentCategory,
                        item.Category,
                        StringComparison.OrdinalIgnoreCase))
                {
                    currentCategory = item.Category;

                    DrawCategoryHeader(currentCategory);
                }

                if (GUILayout.Button(
                        new GUIContent(
                            item.Label,
                            item.Tooltip),
                        EditorStyles.miniButton))
                {
                    item.OnSelected?.Invoke();

                    editorWindow.Close();

                    GUIUtility.ExitGUI();
                }
            }

            if (visibleCount == 0)
            {
                EditorGUILayout.Space(4f);

                EditorGUILayout.LabelField(
                    "No matching properties.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawCategoryHeader(
            string category)
        {
            EditorGUILayout.Space(3f);

            EditorGUILayout.LabelField(
                category,
                EditorStyles.boldLabel);
        }

        private void DrawSearch()
        {
            GUI.SetNextControlName(
                "DynamicProperty.PropertyPicker.Search");

            _search = EditorGUILayout.TextField(
                _search,
                EditorStyles.toolbarSearchField);

            if (!_focusSearch)
                return;

            _focusSearch = false;

            EditorGUI.FocusTextInControl(
                "DynamicProperty.PropertyPicker.Search");
        }

        private bool MatchesSearch(Item item)
        {
            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string query = _search.Trim();

            if (Contains(item.Label, query))
                return true;

            if (Contains(item.SearchText, query))
                return true;

            if (Contains(item.Category, query))
                return true;

            return false;
        }

        private static bool Contains(
            string value,
            string query)
        {
            return
                !string.IsNullOrEmpty(value) &&
                value.IndexOf(
                    query,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void HandleKeyboard()
        {
            var evt = Event.current;

            if (evt.type != EventType.KeyDown)
                return;

            if (evt.keyCode != KeyCode.Escape)
                return;

            editorWindow.Close();
            evt.Use();
        }
    }
}