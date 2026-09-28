using DynamicProperty.DataAnnotations;
using System;
using Color = UnityEngine.Color;
using Vector3 = UnityEngine.Vector3;

namespace DynamicProperty.Samples.TypedSchema
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)]
        [InitialValue(100)]
        [MinMax(0, 1000)]
        [PropertyCategory("Basic")]
        int Health { get; }

        [Property(2)]
        [InitialValue(true)]
        [PropertyCategory("Basic")]
        bool IsBoss { get; }

        [Property(3)]
        [InitialValue(WeaponType.Bow)]
        [PropertyCategory("Ability")]
        WeaponType Weapon { get; }

        [Property(4)]
        [DisplayName("Time for Spawn")]
        [PropertyCategory("Spawn")]
        DateTime SpawnTime { get; }

        [Property(5)]
        [Step(60)]
        [PropertyCategory("Spawn")]
        TimeSpan RespawnDelay { get; }

        [Property(6)]
        [PropertyCategory("Spawn")]
        Vector3 SpawnPosition { get; }

        [Property(7)]
        [PropertyCategory("Spawn")]
        Color BodyColor { get; }

        [Property(8)]
        [PropertyCategory("Ability")]
        AttackType AttackType { get; }
    }

    public enum WeaponType
    {
        None,
        Sword,
        Bow,
        Staff
    }

    [Flags]
    public enum AttackType
    {
        Normal = 1,
        Ranged = 2,
        Melee = 4
    }
}
