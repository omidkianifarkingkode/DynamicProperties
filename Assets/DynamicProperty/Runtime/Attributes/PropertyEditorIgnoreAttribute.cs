using System;

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyEditorIgnoreAttribute : Attribute { }

}
