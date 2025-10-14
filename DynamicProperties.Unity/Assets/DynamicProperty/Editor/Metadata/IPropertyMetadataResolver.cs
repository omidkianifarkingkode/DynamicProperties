using System;

namespace DynamicProperty.Editor
{
    public interface IPropertyMetadataResolver
    {
        PropertyMetadata Get(int id);
        string[] GetAllNames();
        int[] GetAllValues();
        Type BoundEnumType { get; }
    }
}
