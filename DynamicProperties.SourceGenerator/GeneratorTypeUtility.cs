namespace DynamicProperty.SourceGen
{
    internal static class GeneratorTypeUtility
    {
        public static string GetPropertySetViewType(
            SchemaModel schema)
        {
            return
                "global::DynamicProperty.PropertySetView<" +
                schema.FullyQualifiedName +
                ">";
        }
    }
}
