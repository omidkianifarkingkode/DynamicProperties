using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal sealed class GeneratorSymbols
    {
        public INamedTypeSymbol PropertySet { get; }

        public INamedTypeSymbol PropertyTypeAttribute { get; }

        public INamedTypeSymbol GroupAttribute { get; }

        public INamedTypeSymbol GroupComponentAttribute { get; }

        public INamedTypeSymbol FlagsAttribute { get; }

        private GeneratorSymbols(
            INamedTypeSymbol propertySet,
            INamedTypeSymbol propertyTypeAttribute,
            INamedTypeSymbol groupAttribute,
            INamedTypeSymbol groupComponentAttribute,
            INamedTypeSymbol flagsAttribute)
        {
            PropertySet = propertySet;
            PropertyTypeAttribute = propertyTypeAttribute;
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
                    groupAttribute,
                    groupComponentAttribute,
                    flagsAttribute);

            missingSymbols = null;
            return true;
        }
    }
}