
# 🧠 Dynamic Properties for Unity

A powerful, metadata-driven property system for Unity.  
Define, store, and edit flexible game data properties (int, float, bool, enum, DateTime, TimeSpan, etc.) — with efficient storage, runtime access in O(1), and intelligent editor integration.  
Built to solve the “sparse data model” problem in large projects.

## 📦 Installation

1. Open your Unity project.
2. In `Packages/manifest.json`, add:

```json
"com.kingkode.dynamic-property": "https://github.com/omidkianifarkingkode/DynamicProperties.git?path=DynamicProperties.Unity/Assets/DynamicProperty"
```

Unity will automatically fetch the package.

## 🧩 Importing the Example

You can explore the included Basic Sample:

1. Open `Window → Package Manager`.
2. Select `Dynamic Property` from the list.
3. Expand the Samples section.
4. Click Import next to Basic Example.

Unity will import the example into:

```text
Assets/Samples/Dynamic Property/1.0.0/Basic
```

Then, create the example asset manually:

1. Go to `Assets → Create → DynamicProperty → Create Sample Character Data`.

The sample includes a metadata-decorated enum and a ready-to-use `CharacterData` ScriptableObject.

## 🧩 Why Dynamic Properties?

Traditional Unity data models often define dozens of serialized fields — but most remain unused.  
This leads to sparse data models: many fields, few meaningful values, wasted memory, and rigid code.  

Dynamic Properties solve this by storing game data as a list of key-value pairs instead of hard-coded fields.  

However, this raises new challenges — and here’s how the package addresses them:

### ⚙️ 1. Variant Storage (Multi-Type Support)

Storing mixed types in a single list is tricky.  
Dynamic Properties unify all values into:

- `int` (32-bit) for small data (int, float, bool, enums, etc.)
- `long` (64-bit) for large or precise data (double, DateTime, TimeSpan, 64-bit enums)

Other types are converted seamlessly to one of these primitives.  
This keeps storage compact, serialization-safe, and fast.

### ⚡ 2. O(1) Property Lookup

Naively iterating a list for every property lookup is slow.  
Dynamic Properties automatically build a dictionary cache (`PropertySet`) after Unity loads, giving:

- O(1) access time for reads and writes — even across thousands of entries.

### 🧰 3. Metadata-Driven Editor

Instead of hardcoding how each property is drawn, Dynamic Properties use attributes on your enum definitions:

```csharp
public enum CharacterProperties
{
    [PropertyType(typeof(int)), MinMax(0, 1000)] Health,
    [PropertyType(typeof(bool))] IsBoss,
    [PropertyType(typeof(DateTime))] SpawnTime
}
```

The editor automatically:
- Displays the right drawer for each type.
- Applies min/max or step constraints.
- Groups related values (e.g., Vector3, Color).
- No custom inspector code required.

### 🧬 4. Strongly-Typed Access via Source Generator

Using raw keys like `GetInt(Health)` works but isn’t ergonomic.  
Dynamic Properties include a Roslyn Source Generator that creates extension methods based on your enum definitions.

Example:

```csharp
// Your enum
[PropertyType(typeof(int))] Health,
[PropertyType(typeof(bool))] IsBoss,

// Generated
set.Health();
set.SetHealth(50);
set.HasHealth();

set.IsBoss();
set.SetIsBoss(true);
```

Grouped values (like Vector3 or Color) also get high-level accessors:

```csharp
set.GetSpawnPosition();   // Returns Vector3
set.SetSpawnPosition(v3);
```

This means type-safe, auto-complete-friendly, semantic code — no manual boilerplate.

### Prototype: Typed Interface Schemas

Dynamic Properties also includes a prototype typed C# schema declaration model. It is additive; existing enum schemas remain supported.

```csharp
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using Vector3 = UnityEngine.Vector3;

public interface CharacterSchema : IPropertySchema
{
    [Property(1)]
    [InitialValue(100)]
    [MinMax(0, 1000)]
    int Health { get; }

    [Property(6, 7, 8)]
    Vector3 SpawnPosition { get; }
}
```

Typed schemas must be interfaces implementing `IPropertySchema`. Every schema property must declare `[Property(...)]`; missing attributes, unsupported types, invalid aggregate ID counts, and duplicate IDs are reported by the source generator. Aggregate IDs are explicit and ordered: `Vector3` is `x, y, z`, and `Color` is `r, g, b, a`.

In Unity files that import `UnityEngine`, `[Property]` can be ambiguous with `UnityEngine.PropertyAttribute`. Use targeted aliases like `using Vector3 = UnityEngine.Vector3;` or alias the schema attribute if needed.

## 🚀 Usage Overview

### 1️⃣ Define your enum

```csharp
public enum CharacterProperties
{
    [PropertyType(typeof(int)), MinMax(0, 1000)]
    Health,

    [PropertyType(typeof(bool))]
    IsBoss,

    [PropertyType(typeof(Vector3)), Group("Spawn Position")]
    PosX, PosY, PosZ
}
```

### 2️⃣ Bind your enum

Go to `Edit → Project Settings → Dynamic Properties` and assign your enum type (`CharacterProperties`).

### 3️⃣ Use it in data objects

```csharp
[CreateAssetMenu(menuName = "Data/Character")]
public class CharacterData : ScriptableObject
{
    public DynamicProperty.PropertySet Properties;
}
```

### 4️⃣ Access values in code

```csharp
// Generated accessors
int health = Properties.Health();
Properties.SetHealth(80);

Vector3 pos = Properties.GetSpawnPosition();
Properties.SetSpawnPosition(new Vector3(0, 1, 0));
```

You can enable or disable the DynamicProperty Source Generator anytime from:
```text
Tools → Dynamic Property → Source Generator → Enabled in Unity.
```

## 🧱 Key Features

- ✅ Strongly-typed & attribute-driven property metadata
- ⚡ O(1) runtime lookups with auto dictionary serialization
- 🧩 Extensible editor drawers
- 🧠 Source generator for semantic property access
- 🧮 Compact storage (32/64-bit core types)
- 💾 Works in runtime & editor assemblies

## 📄 License

MIT License © 2025 Omid Kianifar (@KingKode)
