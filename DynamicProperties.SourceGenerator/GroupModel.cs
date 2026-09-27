using System.Collections.Generic;

namespace DynamicProperty.SourceGen
{
    internal sealed class GroupModel
    {
        public string Name { get; }

        public AggregateKind Kind { get; }

        /// <summary>
        /// Components are always stored in canonical order:
        ///
        /// Vector2 -> X,Y
        /// Vector3 -> X,Y,Z
        /// Vector4 -> X,Y,Z,W
        /// Color   -> R,G,B,A
        /// </summary>
        public IReadOnlyList<PropertyModel> Components { get; }

        public GroupModel(
            string name,
            AggregateKind kind,
            IReadOnlyList<PropertyModel> components)
        {
            Name = name;
            Kind = kind;
            Components = components;
        }
    }
}