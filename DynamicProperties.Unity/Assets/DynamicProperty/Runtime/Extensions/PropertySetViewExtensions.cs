using System;

namespace DynamicProperty
{
    public static class PropertySetViewExtensions
    {
        public static PropertySetView<TSchema> For<TSchema>(this PropertySet propertySet)
        {
            if (propertySet == null)
                throw new ArgumentNullException(nameof(propertySet));

            return new PropertySetView<TSchema>(propertySet);
        }
    }
}
