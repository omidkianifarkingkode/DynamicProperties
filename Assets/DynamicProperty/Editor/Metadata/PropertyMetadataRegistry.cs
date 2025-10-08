using System;

namespace DynamicProperty.Editor
{
    public static class PropertyMetadataRegistry
    {
        public static IPropertyMetadataResolver Resolver { get; private set; }

        public static void Bind(Type enumType)
        {
            Resolver = enumType != null ? new ReflectionMetadataResolver(enumType) : null;
        }

        /// Call this before using Resolver if you're unsure it's bound.
        public static void EnsureBound()
        {
            if (Resolver != null) return;
            Bind(DynamicPropertiesSettings.instance.EnumType);
        }
    }
}
