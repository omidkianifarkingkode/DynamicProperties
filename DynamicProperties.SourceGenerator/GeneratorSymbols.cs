using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal sealed class GeneratorSymbols
    {
        public INamedTypeSymbol PropertySet { get; }

        public INamedTypeSymbol PropertyAttribute { get; }

        public INamedTypeSymbol PropertySchema { get; }

        public INamedTypeSymbol FlagsAttribute { get; }

        private GeneratorSymbols(
            INamedTypeSymbol propertySet,
            INamedTypeSymbol propertyAttribute,
            INamedTypeSymbol propertySchema,
            INamedTypeSymbol flagsAttribute)
        {
            PropertySet = propertySet;
            PropertyAttribute = propertyAttribute;
            PropertySchema = propertySchema;
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

            var propertyAttribute =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.DataAnnotations.PropertyAttribute");

            var propertySchema =
                compilation.GetTypeByMetadataName(
                    "DynamicProperty.IPropertySchema");

            var flagsAttribute =
                compilation.GetTypeByMetadataName(
                    "System.FlagsAttribute");

            var missing =
                new System.Collections.Generic.List<string>();

            if (propertySet == null)
                missing.Add("DynamicProperty.PropertySet");

            if (propertyAttribute == null)
                missing.Add(
                    "DynamicProperty.DataAnnotations.PropertyAttribute");

            if (propertySchema == null)
                missing.Add("DynamicProperty.IPropertySchema");

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
                    propertyAttribute,
                    propertySchema,
                    flagsAttribute);

            missingSymbols = null;
            return true;
        }
    }
}
