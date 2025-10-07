using System;

public enum PropertyKey : byte
{
    None = 0,

    [DisplayName("Health"), PropertyType(PropertyType.Float), MinMax(0, 100), Step(5)]
    Health = 1,

    [DisplayName("Is Boss"), PropertyType(PropertyType.Bool)]
    IsBoss = 2,

    [DisplayName("Movement Speed"), PropertyType(PropertyType.Float), MinMax(0, 50), Step(0.1f)]
    MoveSpeed = 3,

    [DisplayName("Power Level"), PropertyType(PropertyType.Int), MinMax(0, 999)]
    PowerLevel = 4,

    [PropertyType(PropertyType.Float)]
    PosX = 5,
    [PropertyType(PropertyType.Float)]
    PosY = 6,

    [DisplayName("Spawn Time"), PropertyType(PropertyType.DateTime)]
    SpawnTime = 7,

    [PropertyType(PropertyType.TimeSpan)]
    Cooldown = 8,

    [PropertyType(PropertyType.Enum), PropertyEnum(typeof(WeaponType))]
    WeaponType = 9,

    [PropertyType(PropertyType.Enum), PropertyEnum(typeof(Tag))]
    Tag = 10,
}

public enum WeaponType
{
    None = 0,
    Sword = 1,
    Bow = 2,
    Staff = 3,
}

[Flags]
public enum Tag
{
    None = 0,
    Tag1 = 1,
    Tag2 = 2,
    Tag3 = 4,
}
