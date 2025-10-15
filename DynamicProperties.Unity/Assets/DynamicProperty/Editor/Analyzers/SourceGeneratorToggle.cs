// Assets/DynamicProperty/Editor/Analyzers/SourceGeneratorToggle.cs
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DynamicProperty.Editor
{
    internal static class SourceGeneratorToggle
    {
        private const string AnalyzerFileName = "DynamicProperty.SourceGen.dll";
        private const string AnalyzerLabel = "RoslynAnalyzer";
        private const string MenuPath = "Tools/Dynamic Property/Source Generator/Enabled";
        private const string PrefsKeyGuid = "DynamicProperty_SourceGen_GUID";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Menu.SetChecked(MenuPath, IsEnabled());
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            var path = FindAnalyzerPath();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"[DynamicProperty] Analyzer DLL not found: {AnalyzerFileName}");
                return;
            }

            // If it lives under /Analyzers/, warn that path-based discovery ignores the label toggle.
            if (path.Contains("/Analyzers/"))
            {
                Debug.LogWarning("[DynamicProperty] Analyzer is under a folder named 'Analyzers'. " +
                                 "Unity will load it regardless of the RoslynAnalyzer label. " +
                                 "Move the DLL out of '/Analyzers/' to allow enable/disable.");
            }

            bool newState = !IsEnabled();
            SetEnabled(newState);
            Menu.SetChecked(MenuPath, newState);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem(MenuPath, validate = true)]
        private static bool Validate() => true;

        private static bool IsEnabled()
        {
            var path = FindAnalyzerPath();
            if (string.IsNullOrEmpty(path)) return false;

            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return false;

            var labels = AssetDatabase.GetLabels(obj);
            return labels.Contains(AnalyzerLabel);
        }

        private static void SetEnabled(bool enable)
        {
            var path = FindAnalyzerPath();
            if (string.IsNullOrEmpty(path)) return;

            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return;

            var labels = AssetDatabase.GetLabels(obj).ToList();

            if (enable)
            {
                if (!labels.Contains(AnalyzerLabel))
                {
                    labels.Add(AnalyzerLabel);
                    AssetDatabase.SetLabels(obj, labels.ToArray());
                    Debug.Log("[DynamicProperty] Source Generator ENABLED.");
                }
            }
            else
            {
                if (labels.Contains(AnalyzerLabel))
                {
                    labels.RemoveAll(l => l == AnalyzerLabel);
                    AssetDatabase.SetLabels(obj, labels.ToArray());
                    Debug.Log("[DynamicProperty] Source Generator DISABLED.");
                }
            }
        }

        /// Robust finder that works for Assets/ and Packages/, with label removed.
        private static string FindAnalyzerPath()
        {
            // 1) Try cached GUID -> path
            var cachedGuid = EditorPrefs.GetString(PrefsKeyGuid, string.Empty);
            if (!string.IsNullOrEmpty(cachedGuid))
            {
                var cachedPath = AssetDatabase.GUIDToAssetPath(cachedGuid);
                if (!string.IsNullOrEmpty(cachedPath) && cachedPath.EndsWith("/" + AnalyzerFileName) &&
                    File.Exists(cachedPath))
                {
                    return cachedPath;
                }
            }

            // 2) Exact filename (WITHOUT quotes) to search both Assets/ and Packages/
            var nameGuids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(AnalyzerFileName));
            foreach (var guid in nameGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith("/" + AnalyzerFileName))
                {
                    EditorPrefs.SetString(PrefsKeyGuid, guid);
                    return p;
                }
            }

            // 3) By label (in case it’s still labeled)
            var labeledGuids = AssetDatabase.FindAssets($"l:{AnalyzerLabel}");
            foreach (var guid in labeledGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith("/" + AnalyzerFileName))
                {
                    EditorPrefs.SetString(PrefsKeyGuid, guid);
                    return p;
                }
            }

            // 4) Slow path: enumerate all dlls and match filename
            var allDllGuids = AssetDatabase.FindAssets("t:DefaultAsset");
            foreach (var guid in allDllGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith("/" + AnalyzerFileName))
                {
                    EditorPrefs.SetString(PrefsKeyGuid, guid);
                    return p;
                }
            }

            // Not found — clear cache
            if (!string.IsNullOrEmpty(cachedGuid))
                EditorPrefs.DeleteKey(PrefsKeyGuid);

            return null;
        }
    }
}
#endif
