using DynamicProperty.DataAnnotations;
using System;
using UnityEngine;

namespace DynamicProperty.Samples.Basic
{

    public enum CharacterProperties
    {
        [PropertyEditorIgnore]
        None = 0,

        [PropertyCategory("Basic")]
        [PropertyType(typeof(int), 100), MinMax(0, 1000)]
        Health = 1,

        [PropertyCategory("Basic")]
        [PropertyType(typeof(bool), true)]
        IsBoss = 2,

        [PropertyCategory("Ability")]
        [PropertyType(typeof(WeaponType), WeaponType.Bow)]
        Weapon = 3,

        [PropertyCategory("Spawn")]
        [PropertyType(typeof(DateTime)), DisplayName("Time for Spawn")]
        SpawnTime = 4,

        [PropertyCategory("Spawn")]
        [PropertyType(typeof(TimeSpan)), Step(60)]
        RespawnDelay = 5,

        [PropertyCategory("Spawn")]
        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.X)]
        PosX = 6,
        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.Y)]
        PosY = 7,
        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.Z)]
        PosZ = 8,

        [PropertyCategory("Spawn")]
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.R)]
        ColorR = 10,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.G)]
        ColorG = 11,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.B)]
        ColorB = 12,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.A)]
        ColorA = 13,

        [PropertyCategory("Ability")]
        [PropertyType(typeof(AttackType))]
        AttackType = 14,

        [PropertyCategory("Ability")]
        [PropertyType(typeof(DamageType))]
        DamageType = 15,

        [PropertyCategory("Ability")]
        [PropertyType(typeof(EnemyType))]
        EnemyType = 16,
    }

    public enum WeaponType { None, Sword, Bow, Staff }

    [Flags]
    public enum AttackType { Normal = 1, Ranged = 2, Melee = 4 }

    [Flags]
    public enum DamageType { Normal = 1, Fire = 2, Ice = 4 }

    [Flags]
    public enum EnemyType { Normal = 1, Elite = 2, Boss = 4, NoneBoss = Normal | Elite }
}