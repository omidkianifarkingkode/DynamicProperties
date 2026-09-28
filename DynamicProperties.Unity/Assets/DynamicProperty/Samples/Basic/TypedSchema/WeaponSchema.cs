using DynamicProperty.DataAnnotations;

namespace DynamicProperty.Samples.TypedSchema
{
    public interface WeaponSchema : IPropertySchema
    {
        [Property(1)]
        int Damage { get; }

        [Property(2)]
        bool Melee { get; }

        [Property(3)]
        int Health { get; }
    }
}
