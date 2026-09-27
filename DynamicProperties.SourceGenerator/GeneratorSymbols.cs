using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal sealed class GeneratorSymbols
    {
        public INamedTypeSymbol PropertySet { get; }

        public INamedTypeSymbol PropertyTypeAttribute { get; }

        public INamedTypeSymbol PropertyAttribute { get; }

        public INamedTypeSymbol InitialValueAttribute { get; }

        public INamedTypeSymbol PropertySchema { get; }

        public INamedTypeSymbol GroupAttribute { get; }

        public INamedTypeSymbol GroupComponentAttribute { get; }

        public INamedTypeSymbol FlagsAttribute { get; }

        private GeneratorSymbols(
            INamedTypeSymbol propertySet,
            INamedTypeSymbol propertyTypeAttribute,
            INamedTypeSymbol propertyAttribute,
            INamedTypeSymbol initialValueAttribute,
            INamedTypeSymbol propertySchema,
            INamedTypeSymbol groupAttribute,
            INamedTypeSymbol groupComponentAttribute,
            INamedTypeSymbol flagsAttribute)
        {
            PropertySet = propertySet;
            PropertyTypeAttribute = propertyTypeAttribute;
            PropertyAttribute = propertyAttribute;
            InitialValueAttribute = initialValueAttribute;
            PropertySchema = propertySchema;
            GroupAttribute = groupAttribute;
            GroupComponentAttribute = groupComponentAttribute;
            FlagsAttribute = flagsAttribute;
        }

        public static bool TryCreate(
            Compilation compilation,
            out GeneratorSymbols symbols,
            out string missingSymbols)
        {
            var propertySet =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.PropertySet");

            var propertyTypeAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.PropertyTypeAttribute");

            var propertyAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.PropertyAttribute");

            var initialValueAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.InitialValueAttribute");

            var propertySchema =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.IPropertySchema");

            var groupAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.GroupAttribute");

            var groupComponentAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.GroupComponentAttribute");

            var flagsAttribute =
                compilation.GetTypeByMetadataName(
                    "System.FlagsAttribute");

            var missing =
                new System.Collections.Generic.List<string>();

            if (propertySet == null)
                missing.Add("DynamicProperty.PropertySet");

            if (propertyTypeAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.PropertyTypeAttribute");

            if (propertyAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.PropertyAttribute");

            if (initialValueAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.InitialValueAttribute");

            if (propertySchema == null)
                missing.Add("DynamicProperty.IPropertySchema");

            if (groupAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.GroupAttribute");

            if (groupComponentAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.GroupComponentAttribute");

            if (flagsAttribute == null)
                missing.Add("System.FlagsAttribute");

            if (missing.Count > 0)
            {
                symbols = null;
                missingSymbols =
                    string.Join(", ", missing);

                return false;
            }

            symbols =
                new GeneratorSymbols(
                    propertySet,
                    propertyTypeAttribute,
                    propertyAttribute,
                    initialValueAttribute,
                    propertySchema,
                    groupAttribute,
                    groupComponentAttribute,
                    flagsAttribute);

            missingSymbols = null;
            return true;
        }
    }
}
