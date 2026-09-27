using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class AggregateGenerationTests
{
    [Test]
    public void Vector3Group_GeneratesCompleteApi()
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
        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.X)]
        PosX = 1,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Y)]
        PosY = 2,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Z)]
        PosZ = 3
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(
            source,
            Does.Contain(
                "SpawnPosition("));

        Assert.That(
            source,
            Does.Contain(
                "TryGetSpawnPosition("));

        Assert.That(
            source,
            Does.Contain(
                "HasSpawnPosition("));

        Assert.That(
            source,
            Does.Contain(
                "SetSpawnPosition("));

        Assert.That(
            source,
            Does.Contain(
                "RemoveSpawnPosition("));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void Vector3Has_RequiresAllComponents()
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
        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.X)]
        PosX = 1,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Y)]
        PosY = 2,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Z)]
        PosZ = 3
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(
            source,
            Does.Contain(
                "ContainsAny((int)global::Game.CharacterProperties.PosX) &&"));

        Assert.That(
            source,
            Does.Contain(
                "ContainsAny((int)global::Game.CharacterProperties.PosY) &&"));

        Assert.That(
            source,
            Does.Not.Contain(
                "ContainsAny((int)global::Game.CharacterProperties.PosX) ||"));
    }

    [Test]
    public void Vector3Group_UsesExplicitCanonicalComponentOrder()
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
        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Z)]
        Banana = 1,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.X)]
        Apple = 2,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Y)]
        Orange = 3
    }
}
""");

        string source =
            result.GeneratedSource;

        int x =
            source.IndexOf(
                "Apple, value.x",
                StringComparison.Ordinal);

        int y =
            source.IndexOf(
                "Orange, value.y",
                StringComparison.Ordinal);

        int z =
            source.IndexOf(
                "Banana, value.z",
                StringComparison.Ordinal);

        Assert.That(x, Is.GreaterThanOrEqualTo(0));
        Assert.That(y, Is.GreaterThan(x));
        Assert.That(z, Is.GreaterThan(y));
    }

    [Test]
    public void SupportedAggregateTypes_GenerateCompleteApis()
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
        [PropertyType(typeof(Vector2)), Group("Velocity"), GroupComponent(PropertyComponent.Y)]
        VelocityY = 1,
        [PropertyType(typeof(Vector2)), Group("Velocity"), GroupComponent(PropertyComponent.X)]
        VelocityX = 2,

        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.Z)]
        SpawnZ = 3,
        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.X)]
        SpawnX = 4,
        [PropertyType(typeof(Vector3)), Group("Spawn Position"), GroupComponent(PropertyComponent.Y)]
        SpawnY = 5,

        [PropertyType(typeof(Vector4)), Group("Rotation Value"), GroupComponent(PropertyComponent.W)]
        RotationW = 6,
        [PropertyType(typeof(Vector4)), Group("Rotation Value"), GroupComponent(PropertyComponent.Y)]
        RotationY = 7,
        [PropertyType(typeof(Vector4)), Group("Rotation Value"), GroupComponent(PropertyComponent.X)]
        RotationX = 8,
        [PropertyType(typeof(Vector4)), Group("Rotation Value"), GroupComponent(PropertyComponent.Z)]
        RotationZ = 9,

        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.A)]
        ColorA = 10,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.B)]
        ColorB = 11,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.R)]
        ColorR = 12,
        [PropertyType(typeof(Color)), Group("Body Color"), GroupComponent(PropertyComponent.G)]
        ColorG = 13
    }
}
""");

        string source =
            result.GeneratedSource;

        AssertCompleteApi(source, "global::UnityEngine.Vector2", "Velocity");
        AssertCompleteApi(source, "global::UnityEngine.Vector3", "SpawnPosition");
        AssertCompleteApi(source, "global::UnityEngine.Vector4", "RotationValue");
        AssertCompleteApi(source, "global::UnityEngine.Color", "BodyColor");

        Assert.That(source.IndexOf("VelocityX, value.x", StringComparison.Ordinal), Is.LessThan(source.IndexOf("VelocityY, value.y", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("SpawnX, value.x", StringComparison.Ordinal), Is.LessThan(source.IndexOf("SpawnY, value.y", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("SpawnY, value.y", StringComparison.Ordinal), Is.LessThan(source.IndexOf("SpawnZ, value.z", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("RotationX, value.x", StringComparison.Ordinal), Is.LessThan(source.IndexOf("RotationY, value.y", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("RotationZ, value.z", StringComparison.Ordinal), Is.LessThan(source.IndexOf("RotationW, value.w", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("ColorR, value.r", StringComparison.Ordinal), Is.LessThan(source.IndexOf("ColorG, value.g", StringComparison.Ordinal)));
        Assert.That(source.IndexOf("ColorB, value.b", StringComparison.Ordinal), Is.LessThan(source.IndexOf("ColorA, value.a", StringComparison.Ordinal)));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void AggregateGetterTryGetAndHas_RequireCompleteGroups()
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
        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.X)]
        PosX = 1,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Y)]
        PosY = 2,

        [PropertyType(typeof(Vector3))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Z)]
        PosZ = 3
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("return TryGetSpawnPosition(set, out var value) ? value : default;"));
        Assert.That(source, Does.Contain("!set.TryGetFloat((int)global::Game.CharacterProperties.PosX, out var c0) ||"));
        Assert.That(source, Does.Contain("!set.TryGetFloat((int)global::Game.CharacterProperties.PosY, out var c1) ||"));
        Assert.That(source, Does.Contain("!set.TryGetFloat((int)global::Game.CharacterProperties.PosZ, out var c2)"));
        Assert.That(source, Does.Contain("value = default;"));
        Assert.That(source, Does.Contain("return false;"));
        Assert.That(source, Does.Contain("new global::UnityEngine.Vector3(c0, c1, c2);"));
        Assert.That(source, Does.Contain("set.ContainsAny((int)global::Game.CharacterProperties.PosX) && set.ContainsAny((int)global::Game.CharacterProperties.PosY) && set.ContainsAny((int)global::Game.CharacterProperties.PosZ);"));
        Assert.That(source, Does.Contain("set.SetFloat((int)global::Game.CharacterProperties.PosX, value.x);"));
        Assert.That(source, Does.Contain("set.SetFloat((int)global::Game.CharacterProperties.PosY, value.y);"));
        Assert.That(source, Does.Contain("set.SetFloat((int)global::Game.CharacterProperties.PosZ, value.z);"));
        Assert.That(source, Does.Contain("set.Remove((int)global::Game.CharacterProperties.PosX);"));
        Assert.That(source, Does.Contain("set.Remove((int)global::Game.CharacterProperties.PosY);"));
        Assert.That(source, Does.Contain("set.Remove((int)global::Game.CharacterProperties.PosZ);"));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    private static void AssertCompleteApi(
        string source,
        string typeName,
        string groupName)
    {
        Assert.That(
            source,
            Does.Contain(
                $"public static {typeName} {groupName}("));

        Assert.That(
            source,
            Does.Contain(
                $"public static bool TryGet{groupName}("));

        Assert.That(
            source,
            Does.Contain(
                $"public static bool Has{groupName}("));

        Assert.That(
            source,
            Does.Contain(
                $"public static void Set{groupName}("));

        Assert.That(
            source,
            Does.Contain(
                $"public static void Remove{groupName}("));
    }
}
