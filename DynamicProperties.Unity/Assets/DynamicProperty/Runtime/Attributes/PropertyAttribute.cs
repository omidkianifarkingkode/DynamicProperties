using System;
using System.Collections.Generic;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyAttribute : Attribute
    {
        readonly int[] _ids;

        public IReadOnlyList<int> Ids => _ids;

        public PropertyAttribute(params int[] ids)
        {
            _ids = ids ?? Array.Empty<int>();
        }
    }
}
