using System;

namespace DynamicProperty
{
    public static class PropertySetViewExtensions
    {
        public static PropertySetView<TSchema> For<TSchema>(this PropertySet propertySet)
            where TSchema : struct, Enum
        {
            if (propertySet == null)
                throw new ArgumentNullException(nameof(propertySet));

            return new PropertySetView<TSchema>(propertySet);
        }
    }
}