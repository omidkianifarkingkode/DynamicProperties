using DynamicProperty;

public enum PropertyId
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
}

public enum WeaponType { None, Sword, Bow, Staff }
