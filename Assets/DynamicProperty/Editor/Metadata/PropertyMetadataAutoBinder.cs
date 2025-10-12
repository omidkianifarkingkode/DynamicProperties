using UnityEditor;

namespace DynamicProperty.Editor
{
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
}
