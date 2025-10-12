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

    [PropertyType(typeof(Vector3), "Spawn Position")]
    PosX = 6,
    [PropertyType(typeof(Vector3), "Spawn Position")]
    PosY = 7,
    [PropertyType(typeof(Vector3), "Spawn Position")]
    PosZ = 8,

    [PropertyType(typeof(Color), "Shadow Color")]
    ColorR = 10,
    [PropertyType(typeof(Color), "Shadow Color")]
    ColorG = 11,
    [PropertyType(typeof(Color), "Shadow Color")]
    ColorB = 12,
    [PropertyType(typeof(Color),"Shadow Color")]
    ColorA = 13,
    [DisplayName("Tag"), PropertyType(typeof(TagType))]
    Tag = 14,
}

public enum WeaponType { None, Sword, Bow, Staff }

[Flags]
public enum TagType { Tag1 = 1, Tag2 = 2, Tag3 = 4 }
