using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class ScalarGenerationTests
{
    [Test]
    public void IntProperty_GeneratesCompleteApi()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(
            source,
            Does.Contain(
                "public static int Health("));

        Assert.That(
            source,
            Does.Contain(
                "public static bool TryGetHealth("));

        Assert.That(
            source,
            Does.Contain(
                "public static bool HasHealth("));

        Assert.That(
            source,
            Does.Contain(
                "public static void SetHealth("));

        Assert.That(
            source,
            Does.Contain(
                "public static void RemoveHealth("));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void Getter_DoesNotUseSchemaInitialValue()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int), 100)]
        Health = 1
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(
            source,
            Does.Contain(
                "TryGetHealth(view, out var value) ? value : default"));

        Assert.That(
            source,
            Does.Not.Contain(
                "? value : 100"));
    }

    [Test]
    public void SupportedScalarTypes_GenerateCompleteApis()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using System;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1,

        [PropertyType(typeof(float))]
        Speed = 2,

        [PropertyType(typeof(bool))]
        IsBoss = 3,

        [PropertyType(typeof(long))]
        Experience = 4,

        [PropertyType(typeof(double))]
        Accuracy = 5,

        [PropertyType(typeof(DateTime))]
        SpawnTime = 6,

        [PropertyType(typeof(TimeSpan))]
        RespawnDelay = 7,

        [PropertyType(typeof(WeaponType))]
        Weapon = 8
    }

    public enum WeaponType
    {
        Sword,
        Bow
    }
}
""");

        string source =
            result.GeneratedSource;

        AssertCompleteApi(source, "int", "Health");
        AssertCompleteApi(source, "float", "Speed");
        AssertCompleteApi(source, "bool", "IsBoss");
        AssertCompleteApi(source, "long", "Experience");
        AssertCompleteApi(source, "double", "Accuracy");
        AssertCompleteApi(source, "global::System.DateTime", "SpawnTime");
        AssertCompleteApi(source, "global::System.TimeSpan", "RespawnDelay");
        AssertCompleteApi(source, "global::Game.WeaponType", "Weapon");

        Assert.That(source, Does.Contain("view.PropertySet.TryGetUtcDateTime((int)global::Game.CharacterProperties.SpawnTime, out value);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetUtcDateTime((int)global::Game.CharacterProperties.SpawnTime, value);"));
        Assert.That(source, Does.Contain("view.PropertySet.TryGetTimeSpan((int)global::Game.CharacterProperties.RespawnDelay, out value);"));
        Assert.That(source, Does.Contain("view.PropertySet.TryGetEnum<global::Game.WeaponType>((int)global::Game.CharacterProperties.Weapon, out value);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetEnum<global::Game.WeaponType>((int)global::Game.CharacterProperties.Weapon, value);"));
        Assert.That(source, Does.Contain("TryGetWeapon(view, out var value) ? value : default;"));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void GeneratedIdentifiers_AreSanitizedAndSymbolReferencesAreEscaped()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        @class = 1,

        [PropertyType(typeof(Vector2))]
        [Group("3D Position")]
        [GroupComponent(PropertyComponent.X)]
        PosX = 2,

        [PropertyType(typeof(Vector2))]
        [Group("3D Position")]
        [GroupComponent(PropertyComponent.Y)]
        PosY = 3,

        [PropertyType(typeof(Vector2))]
        [Group("A.B Value")]
        [GroupComponent(PropertyComponent.X)]
        ValueX = 4,

        [PropertyType(typeof(Vector2))]
        [Group("A.B Value")]
        [GroupComponent(PropertyComponent.Y)]
        ValueY = 5
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("public static int Class("));
        Assert.That(source, Does.Contain("(int)global::Game.CharacterProperties.@class"));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector2 _3DPosition("));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector2 ABValue("));
        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    private static void AssertCompleteApi(
        string source,
        string typeName,
        string propertyName)
    {
        Assert.That(
            source,
            Does.Contain(
                $"public static {typeName} {propertyName}(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view)"));

        Assert.That(
            source,
            Does.Contain(
                $"public static bool TryGet{propertyName}(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view, out {typeName} value)"));

        Assert.That(
            source,
            Does.Contain(
                $"public static bool Has{propertyName}(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view)"));

        Assert.That(
            source,
            Does.Contain(
                $"public static void Set{propertyName}(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view, {typeName} value)"));

        Assert.That(
            source,
            Does.Contain(
                $"public static void Remove{propertyName}(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view)"));
    }
}
