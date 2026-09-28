using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

[TestFixture]
public sealed class TypedSchemaGenerationTests
{
    [Test]
    public void GeneratesScalarAndAggregateApisWithEncodedStorageIds()
    {
        var result = GeneratorTestHelper.Run("""
using System;
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum WeaponType { Sword, Bow }

    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)] int Health { get; }
        [Property(2)] float Speed { get; }
        [Property(3)] bool IsBoss { get; }
        [Property(4)] long Experience { get; }
        [Property(5)] double Threat { get; }
        [Property(6)] DateTime SpawnTime { get; }
        [Property(7)] TimeSpan RespawnDelay { get; }
        [Property(8)] WeaponType Weapon { get; }
        [Property(9)] Vector2 Offset { get; }
        [Property(10)] Vector3 Position { get; }
        [Property(11)] Vector4 Rotation { get; }
        [Property(12)] Color Tint { get; }
    }
}
""");

        AssertNoGeneratorFailures(result);

        string source = result.GeneratedSource;

        Assert.That(source, Does.Contain("PropertySetView<global::Game.CharacterSchema>"));
        Assert.That(source, Does.Contain("public static int Health("));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector3 Position("));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Color Tint("));

        Assert.That(source, Does.Contain("global::DynamicProperty.PropertyStorageId.Encode(1, 0)"));
        Assert.That(source, Does.Contain("global::DynamicProperty.PropertyStorageId.Encode(10, 0)"));
        Assert.That(source, Does.Contain("global::DynamicProperty.PropertyStorageId.Encode(10, 1)"));
        Assert.That(source, Does.Contain("global::DynamicProperty.PropertyStorageId.Encode(10, 2)"));
        Assert.That(source, Does.Contain("global::DynamicProperty.PropertyStorageId.Encode(12, 3)"));

        Assert.That(source, Does.Not.Contain("PositionX"));
        Assert.That(source, Does.Not.Contain("TintR"));
    }

    [Test]
    public void GeneratedAggregateApiRequiresAllComponents()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(6)] Vector3 SpawnPosition { get; }
    }
}
""");

        AssertNoGeneratorFailures(result);

        string source = result.GeneratedSource;

        Assert.That(source, Does.Contain("!view.PropertySet.TryGetFloat(global::DynamicProperty.PropertyStorageId.Encode(6, 0), out var c0) ||"));
        Assert.That(source, Does.Contain("!view.PropertySet.TryGetFloat(global::DynamicProperty.PropertyStorageId.Encode(6, 1), out var c1) ||"));
        Assert.That(source, Does.Contain("!view.PropertySet.TryGetFloat(global::DynamicProperty.PropertyStorageId.Encode(6, 2), out var c2)"));
        Assert.That(source, Does.Contain("value = default;"));
        Assert.That(source, Does.Contain("return false;"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(global::DynamicProperty.PropertyStorageId.Encode(6, 0), value.x);"));
        Assert.That(source, Does.Contain("view.PropertySet.Remove(global::DynamicProperty.PropertyStorageId.Encode(6, 2));"));
    }

    [Test]
    public void FlagsEnumPropertyGeneratesFlagHelpersUsingEncodedId()
    {
        var result = GeneratorTestHelper.Run("""
using System;
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    [Flags]
    public enum AttackType
    {
        None = 0,
        Normal = 1,
        Ranged = 2
    }

    public interface CharacterSchema : IPropertySchema
    {
        [Property(8)] AttackType AttackType { get; }
    }
}
""");

        AssertNoGeneratorFailures(result);

        string source = result.GeneratedSource;

        Assert.That(source, Does.Contain("public static bool IsAttackTypeNormal("));
        Assert.That(source, Does.Contain("public static bool IsAttackTypeRanged("));
        Assert.That(source, Does.Contain("view.PropertySet.HasEnumFlag(global::DynamicProperty.PropertyStorageId.Encode(8, 0), global::Game.AttackType.Normal);"));
    }

    [Test]
    public void OrdinaryEnumsAreNotDiscoveredAsSchemas()
    {
        var result = GeneratorTestHelper.Run("""
namespace Game
{
    public enum OrdinaryEnum
    {
        Health = 1
    }
}
""");

        Assert.That(result.GeneratorDiagnostics, Is.Empty);
        Assert.That(result.GeneratedSource, Is.Empty);
    }

    private static void AssertNoGeneratorFailures(GeneratorTestResult result)
    {
        Assert.That(
            result.GeneratorDiagnostics.Select(x => x.ToString()),
            Is.Empty);

        Assert.That(
            result.CompilationErrors.Select(x => x.ToString()),
            Is.Empty);
    }
}
