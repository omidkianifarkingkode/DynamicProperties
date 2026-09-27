using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class ScalarEmitter
    {
        public static void Emit(
            StringBuilder sb,
            PropertyModel property,
            string generatedName,
            string idExpression)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (property == null)
                throw new ArgumentNullException(nameof(property));

            string typeName =
                GetClrTypeName(property);

            EmitGetter(
                sb,
                generatedName,
                typeName);

            EmitTryGet(
                sb,
                property,
                generatedName,
                typeName,
                idExpression);

            EmitHas(
                sb,
                generatedName,
                idExpression);

            EmitSetter(
                sb,
                property,
                generatedName,
                typeName,
                idExpression);

            EmitRemove(
                sb,
                generatedName,
                idExpression);
        }

        private static void EmitGetter(
            StringBuilder sb,
            string name,
            string typeName)
        {
            sb.Append("        public static ")
                .Append(typeName)
                .Append(' ')
                .Append(name)
                .Append("(this DynamicProperty.PropertySet set)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append("            return TryGet")
                .Append(name)
                .Append("(set, out var value) ? value : default;")
                .AppendLine();

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitTryGet(
            StringBuilder sb,
            PropertyModel property,
            string name,
            string typeName,
            string idExpression)
        {
            sb.Append("        public static bool TryGet")
                .Append(name)
                .Append("(this DynamicProperty.PropertySet set, out ")
                .Append(typeName)
                .Append(" value)")
                .AppendLine();

            sb.AppendLine("        {");

            switch (property.Kind)
            {
                case ValueKind.Int32:
                    sb.Append("            return set.TryGetInt(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Single:
                    sb.Append("            return set.TryGetFloat(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Boolean:
                    sb.Append("            return set.TryGetBool(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Int64:
                    sb.Append("            return set.TryGetLong(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Double:
                    sb.Append("            return set.TryGetDouble(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.DateTime:
                    sb.Append("            return set.TryGetUtcDateTime(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.TimeSpan:
                    sb.Append("            return set.TryGetTimeSpan(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Enum:
                    {
                        string enumType =
                            property.DeclaredType.ToDisplayString(
                                SymbolDisplayFormat.FullyQualifiedFormat);

                        sb.Append("            return set.TryGetEnum<")
                            .Append(enumType)
                            .Append(">(")
                            .Append(idExpression)
                            .AppendLine(", out value);");

                        break;
                    }

                default:
                    throw new InvalidOperationException(
                        $"Unsupported scalar value kind '{property.Kind}'.");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitHas(
            StringBuilder sb,
            string name,
            string idExpression)
        {
            sb.Append("        public static bool Has")
                .Append(name)
                .Append("(this DynamicProperty.PropertySet set)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append("            return set.ContainsAny(")
                .Append(idExpression)
                .AppendLine(");");

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitSetter(
            StringBuilder sb,
            PropertyModel property,
            string name,
            string typeName,
            string idExpression)
        {
            sb.Append("        public static void Set")
                .Append(name)
                .Append("(this DynamicProperty.PropertySet set, ")
                .Append(typeName)
                .Append(" value)")
                .AppendLine();

            sb.AppendLine("        {");

            switch (property.Kind)
            {
                case ValueKind.Int32:
                    EmitSetterCall(
                        sb,
                        "SetInt",
                        idExpression);
                    break;

                case ValueKind.Single:
                    EmitSetterCall(
                        sb,
                        "SetFloat",
                        idExpression);
                    break;

                case ValueKind.Boolean:
                    EmitSetterCall(
                        sb,
                        "SetBool",
                        idExpression);
                    break;

                case ValueKind.Int64:
                    EmitSetterCall(
                        sb,
                        "SetLong",
                        idExpression);
                    break;

                case ValueKind.Double:
                    EmitSetterCall(
                        sb,
                        "SetDouble",
                        idExpression);
                    break;

                case ValueKind.DateTime:
                    // DynamicProperty DateTime contract is UTC-only.
                    EmitSetterCall(
                        sb,
                        "SetUtcDateTime",
                        idExpression);
                    break;

                case ValueKind.TimeSpan:
                    EmitSetterCall(
                        sb,
                        "SetTimeSpan",
                        idExpression);
                    break;

                case ValueKind.Enum:
                    {
                        string enumType =
                            property.DeclaredType.ToDisplayString(
                                SymbolDisplayFormat.FullyQualifiedFormat);

                        sb.Append("            set.SetEnum<")
                            .Append(enumType)
                            .Append(">(")
                            .Append(idExpression)
                            .AppendLine(", value);");

                        break;
                    }

                default:
                    throw new InvalidOperationException(
                        $"Unsupported scalar value kind '{property.Kind}'.");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitSetterCall(
            StringBuilder sb,
            string methodName,
            string idExpression)
        {
            sb.Append("            set.")
                .Append(methodName)
                .Append('(')
                .Append(idExpression)
                .AppendLine(", value);");
        }

        private static void EmitRemove(
            StringBuilder sb,
            string name,
            string idExpression)
        {
            sb.Append("        public static void Remove")
                .Append(name)
                .Append("(this DynamicProperty.PropertySet set)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append("            set.Remove(")
                .Append(idExpression)
                .AppendLine(");");

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static string GetClrTypeName(
            PropertyModel property)
        {
            switch (property.Kind)
            {
                case ValueKind.Int32:
                    return "int";

                case ValueKind.Single:
                    return "float";

                case ValueKind.Boolean:
                    return "bool";

                case ValueKind.Int64:
                    return "long";

                case ValueKind.Double:
                    return "double";

                case ValueKind.DateTime:
                    return "global::System.DateTime";

                case ValueKind.TimeSpan:
                    return "global::System.TimeSpan";

                case ValueKind.Enum:
                    return property.DeclaredType.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat);

                default:
                    throw new InvalidOperationException(
                        $"Unsupported scalar value kind '{property.Kind}'.");
            }
        }
    }
}