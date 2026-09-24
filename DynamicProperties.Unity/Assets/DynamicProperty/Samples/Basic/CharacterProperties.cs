using DynamicProperty.DataAnnotations;
using System;
using UnityEngine;

public enum CharacterProperties
{
    [PropertyEditorIgnore]
    None = 0,

    [PropertyType(typeof(int), 100), MinMax(0, 1000)]
    Health = 1,

    [PropertyType(typeof(bool), true)]
    IsBoss = 2,

    [PropertyType(typeof(WeaponType), WeaponType.Bow)]
    Weapon = 3,

    [PropertyType(typeof(DateTime)), DisplayName("Time for Spawn")]
    SpawnTime = 4,

    [PropertyType(typeof(TimeSpan)), Step(60)]
    RespawnDelay = 5,

    [PropertyType(typeof(Vector3)), Group("Spawn Position")]
    PosX = 6,
    [PropertyType(typeof(Vector3)), Group("Spawn Position")]
    PosY = 7,
    [PropertyType(typeof(Vector3)), Group("Spawn Position")]
    PosZ = 8,

    [PropertyType(typeof(Color)), Group("Shadow Color")]
    ColorR = 10,
    [PropertyType(typeof(Color)), Group("Shadow Color")]
    ColorG = 11,
    [PropertyType(typeof(Color)), Group("Shadow Color")]
    ColorB = 12,
    [PropertyType(typeof(Color)), Group("Shadow Color")]
    ColorA = 13,
    [PropertyType(typeof(AttackType))]
    AttackType = 14,
    [PropertyType(typeof(DamageType))]
    DamageType = 15,
    [PropertyType(typeof(EnemyType))]
    EnemyType = 16,
}

public enum WeaponType { None, Sword, Bow, Staff }

[Flags]
public enum AttackType { Normal = 1, Ranged = 2, Melee = 4 }

[Flags]
public enum DamageType { Normal = 1, Fire = 2, Ice = 4 }

[Flags]
public enum EnemyType { Normal = 1, Elite = 2, Boss = 4 }
