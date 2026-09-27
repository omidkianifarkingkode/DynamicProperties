using System;
using System.Text;

namespace DynamicProperty.SourceGen
{
    internal static class AggregateEmitter
    {
        public static void Emit(
            StringBuilder sb,
            SchemaModel schema,
            GroupModel group,
            string generatedName)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (group == null)
                throw new ArgumentNullException(nameof(group));

            string typeName =
                GetAggregateTypeName(group.Kind);

            string receiverType =
                GeneratorTypeUtility.GetPropertySetViewType(schema);

            EmitGetter(
                sb,
                generatedName,
                typeName,
                receiverType);

            EmitTryGet(
                sb,
                schema,
                group,
                generatedName,
                typeName,
                receiverType);

            EmitHas(
                sb,
                schema,
                group,
                generatedName,
                receiverType);

            EmitSetter(
                sb,
                schema,
                group,
                generatedName,
                typeName,
                receiverType);

            EmitRemove(
                sb,
                schema,
                group,
                generatedName,
                receiverType);
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
            SchemaModel schema,
            GroupModel group,
            string name,
            string typeName,
            string receiverType)
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

            for (int i = 0; i < group.Components.Count; i++)
            {
                string id =
                    GetIdExpression(
                        schema,
                        group.Components[i]);

                sb.Append(i == 0
                        ? "            if ("
                        : "                ");

                sb.Append("!view.PropertySet.TryGetFloat(")
                    .Append(id)
                    .Append(", out var c")
                    .Append(i)
                    .Append(')');

                if (i < group.Components.Count - 1)
                {
                    sb.AppendLine(" ||");
                }
                else
                {
                    sb.AppendLine(")");
                }
            }

            sb.AppendLine("            {");
            sb.AppendLine("                value = default;");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine();

            sb.Append("            value = new ")
                .Append(typeName)
                .Append('(');

            for (int i = 0; i < group.Components.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb.Append("c").Append(i);
            }

            sb.AppendLine(");");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitHas(
            StringBuilder sb,
            SchemaModel schema,
            GroupModel group,
            string name,
            string receiverType)
        {
            sb.Append("        public static bool Has")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view)")
                .AppendLine();

            sb.AppendLine("        {");
            sb.Append("            return ");

            for (int i = 0; i < group.Components.Count; i++)
            {
                if (i > 0)
                    sb.Append(" && ");

                sb.Append("view.PropertySet.ContainsAny(")
                    .Append(
                        GetIdExpression(
                            schema,
                            group.Components[i]))
                    .Append(')');
            }

            sb.AppendLine(";");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitSetter(
            StringBuilder sb,
            SchemaModel schema,
            GroupModel group,
            string name,
            string typeName,
            string receiverType)
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

            var componentAccess =
                GetComponentAccessors(group.Kind);

            for (int i = 0; i < group.Components.Count; i++)
            {
                sb.Append("            view.PropertySet.SetFloat(")
                    .Append(
                        GetIdExpression(
                            schema,
                            group.Components[i]))
                    .Append(", value.")
                    .Append(componentAccess[i])
                    .AppendLine(");");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitRemove(
            StringBuilder sb,
            SchemaModel schema,
            GroupModel group,
            string name,
            string receiverType)
        {
            sb.Append("        public static void Remove")
                .Append(name)
                .Append("(this ")
                .Append(receiverType)
                .Append(" view)")
                .AppendLine();

            sb.AppendLine("        {");

            foreach (var component in group.Components)
            {
                sb.Append("            view.PropertySet.Remove(")
                    .Append(
                        GetIdExpression(
                            schema,
                            component))
                    .AppendLine(");");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static string GetIdExpression(
            SchemaModel schema,
            PropertyModel property)
        {
            return
                "(int)" +
                schema.FullyQualifiedName +
                "." +
                IdentifierUtility.EscapeIdentifier(
                    property.Name);
        }

        private static string GetAggregateTypeName(
            AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.Vector2:
                    return "global::UnityEngine.Vector2";

                case AggregateKind.Vector3:
                    return "global::UnityEngine.Vector3";

                case AggregateKind.Vector4:
                    return "global::UnityEngine.Vector4";

                case AggregateKind.Color:
                    return "global::UnityEngine.Color";

                default:
                    throw new InvalidOperationException(
                        $"Unsupported aggregate kind '{kind}'.");
            }
        }

        private static string[] GetComponentAccessors(
            AggregateKind kind)
        {
            switch (kind)
            {
                case AggregateKind.Vector2:
                    return new[] { "x", "y" };

                case AggregateKind.Vector3:
                    return new[] { "x", "y", "z" };

                case AggregateKind.Vector4:
                    return new[] { "x", "y", "z", "w" };

                case AggregateKind.Color:
                    return new[] { "r", "g", "b", "a" };

                default:
                    throw new InvalidOperationException(
                        $"Unsupported aggregate kind '{kind}'.");
            }
        }
    }
}
