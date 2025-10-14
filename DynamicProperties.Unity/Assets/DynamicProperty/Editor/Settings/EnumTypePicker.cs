using DynamicProperty.DataAnnotations;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace DynamicProperty.Editor
{
    internal static class EnumTypePicker
    {
        public static Type[] FindCandidateEnums()
        {
            var attrTypes = new[] {
                typeof(PropertyTypeAttribute),
                typeof(DisplayNameAttribute),
                typeof(MinMaxAttribute),
                typeof(StepAttribute),
                typeof(PropertyEnumAttribute),
            };

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => {
                    Type[] types;
                    try { types = a.GetTypes(); } catch { types = Array.Empty<Type>(); }
                    return types;
                })
                .Where(t => t.IsEnum)
                .Where(t => t.Name.Equals("PropertyId", StringComparison.OrdinalIgnoreCase)
                         || t.GetFields(BindingFlags.Public | BindingFlags.Static)
                              .Any(f => f.GetCustomAttributes(inherit: false)
                                          .Any(att => attrTypes.Contains(att.GetType()))))
                .OrderBy(t => t.FullName)
                .ToArray();
        }

        public static Type Popup(string label, Type current)
        {
            var options = FindCandidateEnums();
            var names = options.Select(t => t.FullName).Prepend("<None>").ToArray();
            int currentIndex = 0;
            if (current != null)
            {
                for (int i = 0; i < options.Length; i++)
                    if (options[i] == current) { currentIndex = i + 1; break; }
            }
            int next = EditorGUILayout.Popup(label, currentIndex, names);
            return next == 0 ? null : options[next - 1];
        }
    }
}
