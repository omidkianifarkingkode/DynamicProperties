using System;
using System.Text;

namespace DynamicProperty.SourceGen
{
    internal static class AggregateEmitter
    {
        public static void Emit(
            StringBuilder sb,
            SchemaModel schema,
            PropertyModel property,
            string generatedName)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (property == null)
                throw new ArgumentNullException(nameof(property));

            string typeName =
                AggregateContract.GetAggregateTypeName(
                    property.AggregateKind);

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
                receiverType);

            EmitHas(
                sb,
                property,
                generatedName,
                receiverType);

            EmitSetter(
                sb,
                property,
                generatedName,
                typeName,
                receiverType);

            EmitRemove(
                sb,
                property,
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
            PropertyModel property,
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

            int slots =
                AggregateContract.GetSlotCount(
                    property.AggregateKind);

            for (int i = 0; i < slots; i++)
            {
                sb.Append(i == 0
                        ? "            if ("
                        : "                ");

                sb.Append("!view.PropertySet.TryGetFloat(")
                    .Append(GetStorageIdExpression(property, i))
                    .Append(", out var c")
                    .Append(i)
                    .Append(')');

                if (i < slots - 1)
                    sb.AppendLine(" ||");
                else
                    sb.AppendLine(")");
            }

            sb.AppendLine("            {");
            sb.AppendLine("                value = default;");
            sb.AppendLine("                return false;");
            sb.AppendLine("            }");
            sb.AppendLine();

            sb.Append("            value = new ")
                .Append(typeName)
                .Append('(');

            for (int i = 0; i < slots; i++)
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
            PropertyModel property,
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

            int slots =
                AggregateContract.GetSlotCount(
                    property.AggregateKind);

            for (int i = 0; i < slots; i++)
            {
                if (i > 0)
                    sb.Append(" && ");

                sb.Append("view.PropertySet.ContainsAny(")
                    .Append(GetStorageIdExpression(property, i))
                    .Append(')');
            }

            sb.AppendLine(";");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitSetter(
            StringBuilder sb,
            PropertyModel property,
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
                AggregateContract.GetComponentAccessors(
                    property.AggregateKind);

            for (int i = 0; i < componentAccess.Length; i++)
            {
                sb.Append("            view.PropertySet.SetFloat(")
                    .Append(GetStorageIdExpression(property, i))
                    .Append(", value.")
                    .Append(componentAccess[i])
                    .AppendLine(");");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void EmitRemove(
            StringBuilder sb,
            PropertyModel property,
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

            int slots =
                AggregateContract.GetSlotCount(
                    property.AggregateKind);

            for (int i = 0; i < slots; i++)
            {
                sb.Append("            view.PropertySet.Remove(")
                    .Append(GetStorageIdExpression(property, i))
                    .AppendLine(");");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static string GetStorageIdExpression(
            PropertyModel property,
            int slot)
        {
            return AggregateContract.GetStorageIdExpression(
                property.LogicalId,
                slot);
        }
    }
}
