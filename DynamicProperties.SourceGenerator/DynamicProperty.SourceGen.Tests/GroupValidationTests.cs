using DynamicProperty.SourceGen.Tests;
using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class GroupValidationTests
{
    [Test]
    public void Vector3MissingZ_ReportsDiagnostic()
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
        PosY = 2
    }
}
""");

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == "DP1107"),
            Is.True);

        Assert.That(
            result.GeneratedSource,
            Does.Not.Contain(
                "SpawnPosition("));
    }

    [Test]
    public void SanitizedGroupNameCollision_ReportsDiagnostic()
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
        [PropertyType(typeof(UnityEngine.Vector2))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.X)]
        A = 1,

        [PropertyType(typeof(UnityEngine.Vector2))]
        [Group("Spawn Position")]
        [GroupComponent(PropertyComponent.Y)]
        B = 2,

        [PropertyType(typeof(UnityEngine.Vector2))]
        [Group("Spawn-Position")]
        [GroupComponent(PropertyComponent.X)]
        C = 3,

        [PropertyType(typeof(UnityEngine.Vector2))]
        [Group("Spawn-Position")]
        [GroupComponent(PropertyComponent.Y)]
        D = 4
    }
}
""");

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == "DP1201"),
            Is.True);
    }

    [Test]
    public void SameEnumNameInDifferentNamespaces_UsesUniqueHintNames()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty.DataAnnotations;

namespace GameA
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }
}

namespace GameB
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }
}
""");

        var generatedSources =
            result.RunResult.Results
                .SelectMany(x => x.GeneratedSources)
                .ToArray();

        Assert.That(
            generatedSources.Length,
            Is.EqualTo(2));

        Assert.That(
            generatedSources
                .Select(x => x.HintName)
                .Distinct()
                .Count(),
            Is.EqualTo(2));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void InvalidGroupDefinitions_ReportExpectedDiagnosticsAndSkipAggregateApis()
    {
        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("")]
        [GroupComponent(PropertyComponent.X)]
        PosX = 1
    }
}
""",
            "DP1101",
            "Pos(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        [Group("Stats")]
        [GroupComponent(PropertyComponent.X)]
        Health = 1
    }
}
""",
            "DP1102",
            "Stats(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        VelocityX = 1,

        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.Y)]
        VelocityY = 2
    }
}
""",
            "DP1103",
            "Velocity(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [GroupComponent(PropertyComponent.X)]
        VelocityX = 1
    }
}
""",
            "DP1104",
            "Velocity(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.Z)]
        VelocityZ = 1,

        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.Y)]
        VelocityY = 2
    }
}
""",
            "DP1105",
            "Velocity(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.X)]
        VelocityX = 1,

        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.X)]
        VelocityX2 = 2
    }
}
""",
            "DP1106",
            "Velocity(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Color))]
        [Group("Body Color")]
        [GroupComponent(PropertyComponent.R)]
        ColorR = 1,

        [PropertyType(typeof(Color))]
        [Group("Body Color")]
        [GroupComponent(PropertyComponent.G)]
        ColorG = 2,

        [PropertyType(typeof(Color))]
        [Group("Body Color")]
        [GroupComponent(PropertyComponent.B)]
        ColorB = 3
    }
}
""",
            "DP1107",
            "BodyColor(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("Mixed")]
        [GroupComponent(PropertyComponent.X)]
        A = 1,

        [PropertyType(typeof(Vector3))]
        [Group("Mixed")]
        [GroupComponent(PropertyComponent.Y)]
        B = 2
    }
}
""",
            "DP1108",
            "Mixed(");

        AssertInvalidGroup(
            """
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent((PropertyComponent)999)]
        VelocityX = 1,

        [PropertyType(typeof(Vector2))]
        [Group("Velocity")]
        [GroupComponent(PropertyComponent.Y)]
        VelocityY = 2
    }
}
""",
            "DP1109",
            "Velocity(");
    }

    private static void AssertInvalidGroup(
        string source,
        string diagnosticId,
        string aggregateMethodFragment)
    {
        var result =
            GeneratorTestHelper.Run(source);

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == diagnosticId),
            Is.True);

        Assert.That(
            result.GeneratedSource,
            Does.Not.Contain(
                aggregateMethodFragment));
    }
}
