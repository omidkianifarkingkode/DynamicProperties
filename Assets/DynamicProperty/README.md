# Dynamic Properties for Unity

A flexible system for defining and editing game data properties (int, float, bool, enum, DateTime, TimeSpan, etc.) with metadata-driven drawers in the Unity Inspector.  
Supports **32-bit and 64-bit storage**, custom attributes for type/range/step, and game-specific enums bound via Project Settings.

---

## 📦 Installation

1. Open your Unity project.
2. In `Packages/manifest.json`, add:

```json
"com.kingkode.dynamic-property": "https://github.com/omidkianifarkingkode/DynamicProperties.git?path=Assets/DynamicProperty"
```

3. Unity will fetch the package automatically.

## 🧩 Importing the Example

You can explore the Basic Sample provided with the package:

1. Open Window → Package Manager.

2. Select Dynamic Property from the list.

3. Expand the Samples section.

4. Click Import next to Basic Example.

This will import the example folder from:

Packages/com.kingkode.dynamic-property/Samples~/Basic

into:
```
Assets/Samples/Dynamic Property/1.0.0/Basic
```
The sample includes:

* A CharacterProperties enum with metadata attributes

* A CharacterData ScriptableObject using DynamicProperty32 and DynamicProperty64

After importing, you must create the ScriptableObject manually using Unity’s menu:
```
➡️ Assets → Create → DynamicProperty → Create Sample Character Data
```

Then, select the created asset to see how dynamic properties appear and behave in the Inspector.

## 🚀 Usage & Setup

1. Define your enum for property IDs in your game assembly:

```csharp
public enum CharacterProperties
{
    [PropertyType(PropertyValueType.Int), DisplayName("Health"), MinMax(0, 9999)]
    Health,

    [PropertyType(PropertyValueType.Float), DisplayName("Speed"), MinMax(0, 50), Step(0.1f)]
    Speed,

    [PropertyType(PropertyValueType.Enum), PropertyEnum(typeof(WeaponType))]
    WeaponType,
}
```

2. Bind your enum in Editor
```
Go to Edit → Project Settings → Dynamic Properties.
```
Select your CharacterProperties enum in the dropdown.

The system now reflects your metadata everywhere.

3. Use in ScriptableObjects
Add fields with DynamicProperty32 or DynamicProperty64:

```csharp
[CreateAssetMenu(menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public List<DynamicProperty32> stats32;
    public List<DynamicProperty64> stats64;
}
```

In the Inspector you’ll see dropdowns for keys and value fields adapting automatically.

## 📝 Core Notes

Storage

DynamicProperty32 = int-backed (ints, floats, bools, short enums).

DynamicProperty64 = long-backed (longs, doubles, 64-bit enums, ticks-based DateTimes/TimeSpans).

Metadata
Use attributes (PropertyType, DisplayName, MinMax, Step, PropertyEnum) on your enum fields to drive Inspector behavior.

Editor Persistence
Binding is stored in ProjectSettings/DynamicProperties.asset (no stray assets in your project).
Binding survives domain reloads & recompiles automatically.

Extensible
You can add new property value types and drawers with minimal boilerplate.

## ✅ Example

In the Inspector, with the example PropertyId above:
```
Health shows as an int slider (0–9999).

Speed shows as a float field with step 0.1.

WeaponType shows as a dropdown of enum values.
```
## License
---
This library is under the MIT License.
