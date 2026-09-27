using System;
using System.ComponentModel;

namespace DynamicProperty
{
    public readonly struct PropertySetView<TSchema>
        where TSchema : struct, Enum
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PropertySet PropertySet { get; }

        internal PropertySetView(PropertySet propertySet)
        {
            PropertySet =
                propertySet ??
                throw new ArgumentNullException(
                    nameof(propertySet));
        }
    }
}