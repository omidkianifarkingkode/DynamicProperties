using DynamicProperty.Editor;
using UnityEditor;

[InitializeOnLoad]
public static class PropertyMetadataAutoBinder
{
    static PropertyMetadataAutoBinder()
    {
        PropertyMetadataRegistry.Bind(DynamicPropertiesSettings.instance.EnumType);
        AssemblyReloadEvents.afterAssemblyReload += () =>
            PropertyMetadataRegistry.Bind(DynamicPropertiesSettings.instance.EnumType);
    }
}
