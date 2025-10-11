using DynamicProperty;
using System;

public enum CharacterProperties
{
    None = 0,

    [DisplayName("Health"), PropertyType(PropertyValueType.Float), MinMax(0, 1000), Step(1)]
    Health = 1,

    [DisplayName("Is Boss"), PropertyType(PropertyValueType.Bool)]
    IsBoss = 2,

    [DisplayName("Weapon Type"), PropertyType(PropertyValueType.Enum), PropertyEnum(typeof(WeaponType))]
    WeaponType = 3,

    [DisplayName("Spawn Time"), PropertyType(PropertyValueType.DateTime)]
    SpawnTime = 4,

    [DisplayName("Respawn Delay"), PropertyType(PropertyValueType.TimeSpan), Step(60)]
    RespawnDelay = 5,

    [PropertyType(PropertyValueType.Float), Group("Position")]
    PosX = 6,
    [PropertyType(PropertyValueType.Float), Group("Position")]
    PosY = 7,
    [PropertyType(PropertyValueType.Float), Group("Position")]
    PosZ = 8,
    [PropertyType(PropertyValueType.Float), Group("Color")]
    ColorR = 9,
    [PropertyType(PropertyValueType.Float), Group("Color")]
    ColorG = 10,
    [PropertyType(PropertyValueType.Float), Group("Color")]
    ColorB = 11,
    [PropertyType(PropertyValueType.Float), Group("Color")]
    ColorA = 12,
    [DisplayName("Tag"), PropertyType(PropertyValueType.Enum), PropertyEnum(typeof(Tag))]
    Tag = 13,
}

public enum WeaponType { None, Sword, Bow, Staff }

[Flags]
public enum Tag { Tag1 = 1, Tag2 = 2, Tag3 = 4}
