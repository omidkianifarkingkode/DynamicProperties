# DynamicProperty

Declare schemas as typed C# interfaces:

```csharp
public interface CharacterSchema : IPropertySchema
{
    [Property(1)]
    [InitialValue(100)]
    [MinMax(0, 1000)]
    [PropertyCategory("Basic")]
    int Health { get; }

    [Property(6)]
    [PropertyCategory("Spawn")]
    Vector3 SpawnPosition { get; }

    [Property(7)]
    [PropertyCategory("Spawn")]
    Color BodyColor { get; }
}
```

Then use the generated `PropertySetView<TSchema>` API:

```csharp
var character = properties.For<CharacterSchema>();

character.Health();
character.SetHealth(200);
character.SpawnPosition();
character.SetSpawnPosition(new Vector3(1, 2, 3));
```

Each `[Property(id)]` value is one logical property ID. Scalars and aggregates share the same storage addressing rule through `PropertyStorageId.Encode(logicalId, slot)`. Aggregates use internal slots for their components, so schema authors no longer declare component IDs.

This package no longer supports enum-based schema declarations. Existing assets created with the old schema/addressing model may need to be recreated.
