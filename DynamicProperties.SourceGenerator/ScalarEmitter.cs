using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal static class ScalarEmitter
    {
        public static void Emit(
            StringBuilder sb,
            SchemaModel schema,
            PropertyModel property,
            string generatedName,
            string idExpression)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (property == null)
                throw new ArgumentNullException(nameof(property));

            string typeName =
                GetClrTypeName(property);

            string receiverType =
                GeneratorTypeUtility.GetPropertySetViewType(schema);

            EmitGetter(
                sb,
                generatedName,
                typeName,
                receiverType);

            EmitTryGet(
                sb,
                property,
                generatedName,
                typeName,
                receiverType,
                idExpression);

            EmitHas(
                sb,
                generatedName,
                receiverType,
                idExpression);

            EmitSetter(
                sb,
                property,
                generatedName,
                typeName,
                receiverType,
                idExpression);

            EmitRemove(
                sb,
                generatedName,
                receiverType,
                idExpression);
        }

        private static void EmitGetter(
            StringBuilder sb,
            string name,
            string typeName,
            string receiverType)
        {
            sb.Append("        public static ")
                .Append(typeName)
                .Append(' ')
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append("            return TryGet")
                .Append(name)
                .Append("(view, out var value) ? value : default;")
                .AppendLine();

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitTryGet(
            StringBuilder sb,
            PropertyModel property,
            string name,
            string typeName,
            string receiverType,
            string idExpression)
        {
            sb.Append("        public static bool TryGet")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view, out ")
                .Append(typeName)
                .Append(" value)")
                .AppendLine();

            sb.AppendLine("        {");

            switch (property.Kind)
            {
                case ValueKind.Int32:
                    sb.Append("            return view.PropertySet.TryGetInt(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Single:
                    sb.Append("            return view.PropertySet.TryGetFloat(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Boolean:
                    sb.Append("            return view.PropertySet.TryGetBool(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Int64:
                    sb.Append("            return view.PropertySet.TryGetLong(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Double:
                    sb.Append("            return view.PropertySet.TryGetDouble(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.DateTime:
                    sb.Append("            return view.PropertySet.TryGetUtcDateTime(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.TimeSpan:
                    sb.Append("            return view.PropertySet.TryGetTimeSpan(")
                        .Append(idExpression)
                        .AppendLine(", out value);");
                    break;

                case ValueKind.Enum:
                    {
                        string enumType =
                            property.DeclaredType.ToDisplayString(
                                SymbolDisplayFormat.FullyQualifiedFormat);

                        sb.Append(
                                "            return view.PropertySet.TryGetEnum<")
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
            string receiverType,
            string idExpression)
        {
            sb.Append("        public static bool Has")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append(
                    "            return view.PropertySet.ContainsAny(")
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
            string receiverType,
            string idExpression)
        {
            sb.Append("        public static void Set")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view, ")
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

                        sb.Append("            view.PropertySet.SetEnum<")
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
            sb.Append("            view.PropertySet.")
                .Append(methodName)
                .Append('(')
                .Append(idExpression)
                .AppendLine(", value);");
        }

        private static void EmitRemove(
            StringBuilder sb,
            string name,
            string receiverType,
            string idExpression)
        {
            sb.Append("        public static void Remove")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view)")
                .AppendLine();

            sb.AppendLine("        {");

            sb.Append(
                    "            view.PropertySet.Remove(")
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
