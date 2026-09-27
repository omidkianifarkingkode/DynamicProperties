using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class TypedInterfaceSchemaGenerationTests
{
    [Test]
    public void ValidTypedInterfaceSchema_GeneratesScalarAndAggregateApis()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)]
        [InitialValue(100)]
        int Health { get; }

        [Property(2)]
        [InitialValue(WeaponType.Bow)]
        WeaponType Weapon { get; }

        [Property(10, 11)]
        Vector2 Offset { get; }

        [Property(20, 21, 22)]
        Vector3 Position { get; }

        [Property(30, 31, 32, 33)]
        Vector4 Value { get; }

        [Property(40, 41, 42, 43)]
        Color Tint { get; }
    }

    public enum WeaponType
    {
        Sword,
        Bow
    }

    public static class Consumer
    {
        public static void Test(PropertySet properties)
        {
            var character = properties.For<CharacterSchema>();

            int health = character.Health();
            WeaponType weapon = character.Weapon();
            Vector2 offset = character.Offset();
            Vector3 position = character.Position();
            Vector4 value = character.Value();
            Color tint = character.Tint();

            character.SetHealth(health);
            character.SetWeapon(weapon);
            character.SetOffset(offset);
            character.SetPosition(position);
            character.SetValue(value);
            character.SetTint(tint);
        }
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("public static int Health(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));
        Assert.That(source, Does.Contain("public static global::Game.WeaponType Weapon(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector2 Offset(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector3 Position(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Vector4 Value(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));
        Assert.That(source, Does.Contain("public static global::UnityEngine.Color Tint(this global::DynamicProperty.PropertySetView<global::Game.CharacterSchema> view)"));

        Assert.That(source, Does.Contain("view.PropertySet.TryGetInt(1, out value);"));
        Assert.That(source, Does.Contain("view.PropertySet.TryGetEnum<global::Game.WeaponType>(2, out value);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(20, value.x);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(21, value.y);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(22, value.z);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(40, value.r);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(41, value.g);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(42, value.b);"));
        Assert.That(source, Does.Contain("view.PropertySet.SetFloat(43, value.a);"));
        Assert.That(source, Does.Not.Contain("PositionX("));
        Assert.That(source, Does.Not.Contain("PositionY("));
        Assert.That(source, Does.Not.Contain("PositionZ("));

        Assert.That(result.CompilationErrors, Is.Empty);
    }

    [Test]
    public void InterfaceSchema_DiagnosticsCoverInvalidDeclarations()
    {
        AssertDiagnostic(
            """
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1, 2)]
        Vector3 Position { get; }
    }
}
""",
            "DP1302");

        AssertDiagnostic(
            """
using DynamicProperty;
using DynamicProperty.DataAnnotations;
using UnityEngine;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(6, 6, 8)]
        Vector3 Position { get; }
    }
}
""",
            "DP1303");

        AssertDiagnostic(
            """
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)]
        int Health { get; }

        [Property(1)]
        bool IsBoss { get; }
    }
}
""",
            "DP1304");

        AssertDiagnostic(
            """
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)]
        string Name { get; }
    }
}
""",
            "DP1002");

        AssertDiagnostic(
            """
using DynamicProperty;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        int Health { get; }
    }
}
""",
            "DP1301");
    }

    [Test]
    public void EnumAndInterfaceSchemas_WorkInSameCompilation()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public enum CharacterProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }

    public interface CharacterSchema : IPropertySchema
    {
        [Property(2)]
        bool IsBoss { get; }
    }

    public static class Consumer
    {
        public static void Test(PropertySet properties)
        {
            int health = properties.For<CharacterProperties>().Health();
            bool isBoss = properties.For<CharacterSchema>().IsBoss();
        }
    }
}
""");

        string source =
            result.GeneratedSource;

        Assert.That(source, Does.Contain("PropertySetView<global::Game.CharacterProperties>"));
        Assert.That(source, Does.Contain("PropertySetView<global::Game.CharacterSchema>"));
        Assert.That(result.CompilationErrors, Is.Empty);
    }

    [Test]
    public void TypedInterfaceSchema_UsesGeneratedNameCollisionValidation()
    {
        var result =
            GeneratorTestHelper.Run(
                """
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)]
        int Health { get; }

        [Property(2)]
        int TryGetHealth { get; }
    }
}
""");

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == "DP1201"),
            Is.True);
    }

    private static void AssertDiagnostic(
        string source,
        string diagnosticId)
    {
        var result =
            GeneratorTestHelper.Run(source);

        Assert.That(
            result.GeneratorDiagnostics
                .Any(x => x.Id == diagnosticId),
            Is.True);

        Assert.That(
            result.GeneratedSource,
            Is.Empty);
    }
}
