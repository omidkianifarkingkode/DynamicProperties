using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace DynamicProperty.SourceGen
{
    internal static class IdentifierUtility
    {
        /// <summary>
        /// Used only when referencing an existing C# symbol.
        /// Example: enum member named @class.
        /// </summary>
        public static string EscapeIdentifier(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "_";

            return SyntaxFacts.GetKeywordKind(raw) != SyntaxKind.None
                ? "@" + raw
                : raw;
        }

        /// <summary>
        /// Converts arbitrary user-facing text to a valid generated
        /// Pascal-style C# identifier.
        /// </summary>
        public static string ToPascalIdentifier(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "_";

            var builder = new StringBuilder(raw.Length);

            bool capitalizeNext = true;

            foreach (char ch in raw.Trim())
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    if (builder.Length == 0 &&
                        char.IsDigit(ch))
                    {
                        builder.Append('_');
                    }

                    if (capitalizeNext &&
                        char.IsLetter(ch))
                    {
                        builder.Append(
                            char.ToUpperInvariant(ch));
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    capitalizeNext = false;
                    continue;
                }

                // Spaces, hyphens, dots, etc. become
                // word boundaries.
                capitalizeNext = true;
            }

            if (builder.Length == 0)
                return "_";

            string result =
                builder.ToString();

            if (SyntaxFacts.GetKeywordKind(result) !=
                SyntaxKind.None)
            {
                result = "_" + result;
            }

            return result;
        }

        public static string GetFlagMethodName(
            string propertyGeneratedName,
            string flagName)
        {
            return
                "Is" +
                propertyGeneratedName +
                ToPascalIdentifier(flagName);
        }
    }
}