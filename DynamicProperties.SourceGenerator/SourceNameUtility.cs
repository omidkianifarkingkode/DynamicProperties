using System.Text;

using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class SourceNameUtility
    {
        public static string GetExtensionClassName(
            SchemaModel schema)
        {
            var builder =
                new StringBuilder();

            AppendContainingTypes(
                schema.Symbol,
                builder);

            builder.Append(
                IdentifierUtility.ToPascalIdentifier(
                    schema.Name));

            builder.Append(
                "PropertySetExtensions");

            return builder.ToString();
        }

        public static string GetHintName(
            SchemaModel schema)
        {
            string fullName =
                schema.FullyQualifiedName;

            string readable =
                MakeReadable(fullName);

            ulong hash =
                ComputeStableHash(fullName);

            return
                readable +
                "_" +
                hash.ToString("X16") +
                ".PropertySetExtensions.g.cs";
        }

        private static void AppendContainingTypes(
            INamedTypeSymbol symbol,
            StringBuilder builder)
        {
            if (symbol.ContainingType == null)
                return;

            AppendContainingTypes(
                symbol.ContainingType,
                builder);

            builder.Append(
                IdentifierUtility.ToPascalIdentifier(
                    symbol.ContainingType.Name));
        }

        private static string MakeReadable(
            string value)
        {
            var builder =
                new StringBuilder(value.Length);

            foreach (char ch in value)
            {
                if (char.IsLetterOrDigit(ch))
                    builder.Append(ch);
                else
                    builder.Append('_');
            }

            return builder.ToString();
        }

        private static ulong ComputeStableHash(
            string value)
        {
            // FNV-1a 64 bit.
            const ulong offset =
                14695981039346656037UL;

            const ulong prime =
                1099511628211UL;

            ulong hash = offset;

            foreach (char ch in value)
            {
                hash ^= ch;
                hash *= prime;
            }

            return hash;
        }
    }
}
