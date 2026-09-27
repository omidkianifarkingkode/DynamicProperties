using System.Collections.Generic;

namespace DynamicProperty.Editor
{
    internal sealed class PropertyGroupDefinition
    {
        public string Name { get; }

        public PropertyGroupKind Kind { get; }

        public IReadOnlyList<int> MemberIds { get; }

        public string Error { get; }

        public bool IsValid => string.IsNullOrEmpty(Error);

        public PropertyGroupDefinition(
            string name,
            PropertyGroupKind kind,
            IReadOnlyList<int> memberIds,
            string error)
        {
            Name = name;
            Kind = kind;
            MemberIds = memberIds;
            Error = error;
        }
    }
}