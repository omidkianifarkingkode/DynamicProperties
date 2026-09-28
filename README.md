# DynamicProperty

DynamicProperty stores sparse typed values in a `PropertySet` and generates strongly typed accessors from C# schema interfaces.

## Schema Declaration

Schemas are interfaces implementing `IPropertySchema`. Each property has exactly one logical ID:

```csharp
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
    WeaponType Weapon { get; }

    [Property(6)]
    [PropertyCategory("Spawn")]
    Vector3 SpawnPosition { get; }

    [Property(7)]
    [PropertyCategory("Spawn")]
    Color BodyColor { get; }
}
```

The C# property type defines the DynamicProperty value kind. Supported scalar types are `int`, `float`, `bool`, `long`, `double`, `DateTime`, `TimeSpan`, and enum value types. Supported aggregates are `Vector2`, `Vector3`, `Vector4`, and `Color`.

Generated usage stays centered on `PropertySetView<TSchema>`:

```csharp
var character = properties.For<CharacterSchema>();

int health = character.Health();
character.SetHealth(200);

Vector3 position = character.SpawnPosition();
character.SetSpawnPosition(new Vector3(1, 2, 3));
```

## Storage IDs

`[Property(id)]` is a logical property ID. Internal sparse storage IDs are derived centrally with:

```text
storageId = (logicalId << 3) | slot
```

Slot `0` is used for scalars. Aggregates use slots in component order:

```text
Vector2: X=0, Y=1
Vector3: X=0, Y=1, Z=2
Vector4: X=0, Y=1, Z=2, W=3
Color:   R=0, G=1, B=2, A=3
```

This is a breaking pre-v1 storage redesign. Assets authored with the previous enum/component-ID schema model may need to be recreated; no legacy migration layer is retained.
