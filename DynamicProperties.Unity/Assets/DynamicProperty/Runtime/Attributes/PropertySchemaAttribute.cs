using System;

namespace DynamicProperty.DataAnnotations
{
    /// <summary>
    /// Defines the property schema used by a PropertySet field.
    /// The schema must be an int-backed enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertySchemaAttribute : Attribute
    {
        public Type SchemaType { get; }

        public PropertySchemaAttribute(Type schemaType)
        {
            SchemaType = schemaType;
        }
    }
}
