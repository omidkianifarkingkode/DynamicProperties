using System;
using UnityEditor;

[FilePath("ProjectSettings/DynamicProperties.asset", FilePathAttribute.Location.ProjectFolder)]
public sealed class DynamicPropertiesSettings : ScriptableSingleton<DynamicPropertiesSettings>
{
    public string enumTypeName;
    public Type EnumType => string.IsNullOrEmpty(enumTypeName) ? null : Type.GetType(enumTypeName);

    public void SetEnumType(Type t)
    {
        enumTypeName = t?.AssemblyQualifiedName;
        Save(true);
    }
}
