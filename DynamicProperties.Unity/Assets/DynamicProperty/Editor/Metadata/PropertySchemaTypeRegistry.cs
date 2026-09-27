using DynamicProperty.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicProperty.Editor
{
    internal static class PropertySchemaTypeRegistry
    {
        private static readonly Type[] SchemaTypes = DiscoverSchemaTypes();

        private static readonly Dictionary<string, Type> TypesBySerializedName =
            SchemaTypes
                .Where(type => !string.IsNullOrEmpty(type.AssemblyQualifiedName))
                .ToDictionary(
                    type => type.AssemblyQualifiedName,
                    type => type,
                    StringComparer.Ordinal);

        public static IReadOnlyList<Type> GetAll()
        {
            return SchemaTypes;
        }

        public static bool IsSchemaType(Type type)
        {
            if (type == null)
                return false;

            if (type.IsInterface &&
                typeof(IPropertySchema).IsAssignableFrom(type))
            {
                return type
                    .GetProperties()
                    .Any(property =>
                        property.GetCustomAttribute<DynamicProperty.DataAnnotations.PropertyAttribute>() != null);
            }

            if (type.IsEnum)
            {
                // Original DynamicProperty schema contract.
                if (Enum.GetUnderlyingType(type) != typeof(int))
                    return false;

                return type
                    .GetFields(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static)
                    .Any(field =>
                        field.GetCustomAttribute<PropertyTypeAttribute>() != null);
            }

            return false;
        }

        public static string GetSerializedName(Type type)
        {
            return type?.AssemblyQualifiedName;
        }

        public static Type Resolve(string serializedName)
        {
            if (string.IsNullOrWhiteSpace(serializedName))
                return null;

            if (TypesBySerializedName.TryGetValue(serializedName, out var knownType))
            {
                return knownType;
            }

            // Fallback in case the type was not present when
            // the static cache was initially built.
            var resolved =
                Type.GetType(serializedName, throwOnError: false);

            return IsSchemaType(resolved)
                ? resolved
                : null;
        }

        private static Type[] DiscoverSchemaTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(IsSchemaType)
                .OrderBy(
                    type => type.FullName,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static IEnumerable<Type> GetLoadableTypes(
            Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types
                    .Where(type => type != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }
    }
}
