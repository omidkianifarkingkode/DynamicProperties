using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DynamicProperty.SourceGen
{
    internal enum ValueKind
    {
        Int32,
        Single,
        Boolean,
        Int64,
        Double,
        DateTime,
        TimeSpan,
        Enum,
        Unknown
    }

    internal enum AggregateKind
    {
        None,
        Vector2,
        Vector3,
        Vector4,
        Color
    }

    internal enum GroupComponentKind
    {
        X,
        Y,
        Z,
        W,

        R,
        G,
        B,
        A
    }

    internal sealed class SchemaModel
    {
        public INamedTypeSymbol Symbol { get; }

        public string Name { get; }

        public string Namespace { get; }

        public string FullyQualifiedName { get; }

        public IReadOnlyList<PropertyModel> Properties { get; }

        public SchemaModel(
            INamedTypeSymbol symbol,
            string name,
            string @namespace,
            string fullyQualifiedName,
            IReadOnlyList<PropertyModel> properties)
        {
            Symbol = symbol;
            Name = name;
            Namespace = @namespace;
            FullyQualifiedName = fullyQualifiedName;
            Properties = properties;
        }
    }

    internal sealed class PropertyModel
    {
        public IFieldSymbol Symbol { get; }

        public string Name { get; }

        public ITypeSymbol DeclaredType { get; }

        public ValueKind Kind { get; }

        public AggregateKind AggregateKind { get; }

        public bool IsFlagsEnum { get; }

        public bool HasGroupAttribute { get; }

        public string GroupName { get; }

        public bool HasGroupComponentAttribute { get; }

        public GroupComponentKind? GroupComponent { get; }

        public PropertyModel(
            IFieldSymbol symbol,
            string name,
            ITypeSymbol declaredType,
            ValueKind kind,
            AggregateKind aggregateKind,
            bool isFlagsEnum,
            bool hasGroupAttribute,
            string groupName,
            bool hasGroupComponentAttribute,
            GroupComponentKind? groupComponent)
        {
            Symbol = symbol;
            Name = name;
            DeclaredType = declaredType;
            Kind = kind;
            AggregateKind = aggregateKind;
            IsFlagsEnum = isFlagsEnum;
            HasGroupAttribute = hasGroupAttribute;
            GroupName = groupName;
            HasGroupComponentAttribute = hasGroupComponentAttribute;
            GroupComponent = groupComponent;
        }
    }
}