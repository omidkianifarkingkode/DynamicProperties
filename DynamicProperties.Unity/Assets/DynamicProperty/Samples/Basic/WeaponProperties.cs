using DynamicProperty.DataAnnotations;

namespace DynamicProperty.Samples.Basic
{
    public enum WeaponProperties 
    {
        [PropertyEditorIgnore]
        None = 0,

        [PropertyType(typeof(int), 100), MinMax(0, 1000)]
        Deamage = 1,

        [PropertyType(typeof(bool), true)]
        Meele = 2,
    }
}