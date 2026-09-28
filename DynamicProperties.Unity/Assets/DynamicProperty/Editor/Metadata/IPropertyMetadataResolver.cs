using System;
using System.Collections.Generic;

namespace DynamicProperty.Editor
{
    public interface IPropertyMetadataResolver
    {
        Type SchemaType { get; }
        IReadOnlyList<PropertyMetadata> Properties { get; }
        PropertyMetadata GetByLogicalId(int logicalId);
        PropertyMetadata GetByStorageId(int storageId);
        string GetNameByStorageId(int storageId);
    }
}
