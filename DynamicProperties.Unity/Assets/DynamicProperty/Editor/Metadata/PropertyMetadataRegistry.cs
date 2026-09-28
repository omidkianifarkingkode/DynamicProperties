using System;
using System.Collections.Generic;

namespace DynamicProperty.Editor
{
    public static class PropertyMetadataRegistry
    {
        private static readonly Dictionary<Type, IPropertyMetadataResolver> Resolvers = new();

        public static bool TryGetResolver(Type schemaType, out IPropertyMetadataResolver resolver, out string error)
        {
            resolver = null;
            error = null;

            if (schemaType == null)
            {
                error = "Property schema type is null.";
                return false;
            }

            if (!schemaType.IsInterface ||
                !typeof(IPropertySchema).IsAssignableFrom(schemaType))
            {
                error = $"Property schema '{schemaType.FullName}' must be an interface implementing IPropertySchema.";

                return false;
            }

            if (Resolvers.TryGetValue(schemaType, out resolver))
                return true;

            resolver = new ReflectionMetadataResolver(schemaType);

            Resolvers.Add(schemaType, resolver);

            return true;
        }

        internal static void Clear()
        {
            Resolvers.Clear();
        }
    }
}
