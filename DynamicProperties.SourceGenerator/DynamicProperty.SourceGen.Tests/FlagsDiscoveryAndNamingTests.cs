using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class FlagsDiscoveryAndNamingTests
{
    [Test]
    public void ExplicitFlagsEnum_GeneratesFlagHelpersIncludingZeroAndCompositeValues()
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
        [PropertyType(typeof(AttackType))]
        Attack = 1
    }

    [Flags]
    public enum AttackType
    {
        None = 0,
        Normal = 1,
        Ranged = 2,
        Melee = 4,
        All = Normal | Ranged | Melee
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("public static bool IsAttackNone("));
        Assert.That(source, Does.Contain("public static bool IsAttackNormal("));
        Assert.That(source, Does.Contain("public static bool IsAttackRanged("));
        Assert.That(source, Does.Contain("public static bool IsAttackMelee("));
        Assert.That(source, Does.Contain("public static bool IsAttackAll("));
        Assert.That(source, Does.Contain("public static bool IsAttackNormal(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view)"));
        Assert.That(source, Does.Contain("return view.PropertySet.HasEnumFlag((int)global::Game.CharacterProperties.Attack, global::Game.AttackType.Normal);"));
        Assert.That(source, Does.Contain("global::Game.AttackType.All"));
        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void NonFlagsEnum_DoesNotGenerateFlagHelpers()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(AttackType))]
        Attack = 1
    }

    public enum AttackType
    {
        Normal = 1,
        Ranged = 2,
        Melee = 4
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Not.Contain("IsAttackNormal("));
        Assert.That(source, Does.Not.Contain("IsAttackRanged("));
        Assert.That(source, Does.Not.Contain("IsAttackMelee("));
        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void GeneratedNameCollisions_ReportDiagnostics()
    {
        AssertCollision(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2)), Group("Spawn Position"), GroupComponent(PropertyComponent.X)]
        A = 1,
        [PropertyType(typeof(Vector2)), Group("Spawn Position"), GroupComponent(PropertyComponent.Y)]
        B = 2,
        [PropertyType(typeof(Vector2)), Group("Spawn-Position"), GroupComponent(PropertyComponent.X)]
        C = 3,
        [PropertyType(typeof(Vector2)), Group("Spawn-Position"), GroupComponent(PropertyComponent.Y)]
        D = 4
    }
}
""");

        AssertCollision(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        SpawnPosition = 1,
        [PropertyType(typeof(Vector2)), Group("Spawn Position"), GroupComponent(PropertyComponent.X)]
        X = 2,
        [PropertyType(typeof(Vector2)), Group("Spawn Position"), GroupComponent(PropertyComponent.Y)]
        Y = 3
    }
}
""");

        AssertCollision(
            """
using System;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(AttackType))]
        Attack = 1,
        [PropertyType(typeof(bool))]
        IsAttackNormal = 2
    }

    [Flags]
    public enum AttackType
    {
        Normal = 1
    }
}
""");
    }

    [Test]
    public void Discovery_UsesDynamicPropertyAttributeSymbolIdentity()
    {
        var discovered =
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

        Assert.That(
            discovered.RunResult.Results
                .SelectMany(x => x.GeneratedSources)
                .Count(),
            Is.EqualTo(1));

        var ordinary =
            GeneratorTestHelper.Run(
                """
namespace Game
{
    public enum CharacterProperties
    {
        Health = 1
    }
}
""");

        Assert.That(ordinary.GeneratedSource, Is.Empty);

        var unrelated =
            GeneratorTestHelper.Run(
                """
using System;
using Other;

namespace Other
{
    public sealed class PropertyTypeAttribute : Attribute
    {
        public PropertyTypeAttribute(Type type) { }
    }
}

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }
}
""");

        Assert.That(unrelated.GeneratedSource, Is.Empty);
    }

    [Test]
    public void SameNestedEnumNames_GenerateUniqueReadableExtensionClassesAndHintNames()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;

namespace Game
{
    public class Player
    {
        public enum Properties
        {
            [PropertyType(typeof(int))]
            Health = 1
        }
    }

    public class Enemy
    {
        public enum Properties
        {
            [PropertyType(typeof(int))]
            Health = 1
        }
    }
}
""");

        var generatedSources =
            result.RunResult.Results
                .SelectMany(x => x.GeneratedSources)
                .ToArray();

        Assert.That(generatedSources.Length, Is.EqualTo(2));
        Assert.That(
            generatedSources
                .Select(x => x.HintName)
                .Distinct()
                .Count(),
            Is.EqualTo(2));

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("public static class PlayerPropertiesPropertySetExtensions"));
        Assert.That(source, Does.Contain("public static class EnemyPropertiesPropertySetExtensions"));
        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    private static void AssertCollision(
        string source)
    {
        var result =
            GeneratorTestHelper.Run(source);

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == "DP1201"),
            Is.True);
    }
}
