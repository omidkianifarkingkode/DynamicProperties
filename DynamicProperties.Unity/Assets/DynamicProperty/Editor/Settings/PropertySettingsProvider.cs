using System;
using UnityEditor;

namespace DynamicProperty.Editor
{
    public sealed class PropertySettingsProvider : SettingsProvider
    {
        private const string SettingsPath = "Project/Dynamic Properties";

        public PropertySettingsProvider()
            : base(SettingsPath, SettingsScope.Project) { }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("Dynamic Properties", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bind your game's PropertyId enum type.", MessageType.Info);

            var settings = DynamicPropertiesSettings.instance;

            Type current = settings.EnumType;
            Type newType = EnumTypePicker.Popup("PropertyId Enum", current);

            if (newType != current)
            {
                settings.SetEnumType(newType);          // persists to ProjectSettings
                PropertyMetadataRegistry.Bind(newType); // bind immediately
            }
        }

        [SettingsProvider]
        public static SettingsProvider Create() => new PropertySettingsProvider();
    }
}
