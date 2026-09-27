using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

public sealed class PropertySetViewMigrationTests
{
    [Test]
    public void SamePropertyNameAcrossSchemas_IsNotAmbiguousThroughTypedViews()
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

    public enum WeaponProperties
    {
        [PropertyType(typeof(int))]
        Health = 1
    }

    public static class Consumer
    {
        public static void Test(
            PropertySet properties)
        {
            var character =
                properties.For<CharacterProperties>();

            var weapon =
                properties.For<WeaponProperties>();

            int characterHealth =
                character.Health();

            int weaponHealth =
                weapon.Health();
        }
    }
}
""");

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }

    [Test]
    public void SchemaApis_AreNotGeneratedDirectlyOnPropertySet()
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
                "Health(this global::DynamicProperty.PropertySetView<global::Game.CharacterProperties> view)"));

        Assert.That(
            source,
            Does.Not.Match(
                @"Health\(this (global::)?DynamicProperty\.PropertySet\s"));

        Assert.That(
            result.CompilationErrors,
            Is.Empty);
    }
}
