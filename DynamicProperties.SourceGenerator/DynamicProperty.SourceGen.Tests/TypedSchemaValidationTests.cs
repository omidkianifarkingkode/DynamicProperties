using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

[TestFixture]
public sealed class TypedSchemaValidationTests
{
    [TestCase(0, "DP1302")]
    [TestCase(-1, "DP1302")]
    [TestCase(268435456, "DP1302")]
    public void InvalidLogicalIdsProduceDiagnostics(int id, string diagnosticId)
    {
        var result = GeneratorTestHelper.Run($$"""
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property({{id}})] int Health { get; }
    }
}
""");

        AssertHasDiagnostic(result, diagnosticId);
    }

    [Test]
    public void DuplicateLogicalIdsProduceDiagnostic()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)] int Health { get; }
        [Property(1)] bool IsBoss { get; }
    }
}
""");

        AssertHasDiagnostic(result, "DP1304");
    }

    [Test]
    public void MissingPropertyAttributeProducesDiagnostic()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        int Health { get; }
    }
}
""");

        AssertHasDiagnostic(result, "DP1301");
    }

    [Test]
    public void UnsupportedTypeProducesDiagnostic()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)] string Name { get; }
    }
}
""");

        AssertHasDiagnostic(result, "DP1002");
    }

    [Test]
    public void InvalidSchemaShapesProduceDiagnostics()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)] int this[int index] { get; }
        [Property(2)] int Writable { get; set; }
    }
}
""");

        AssertHasDiagnostic(result, "DP1305");
        AssertHasDiagnostic(result, "DP1307");
    }

    [Test]
    public void GeneratedNameCollisionsProduceDiagnostic()
    {
        var result = GeneratorTestHelper.Run("""
using DynamicProperty;
using DynamicProperty.DataAnnotations;

namespace Game
{
    public interface CharacterSchema : IPropertySchema
    {
        [Property(1)] int Health { get; }
        [Property(2)] int TryGetHealth { get; }
    }
}
""");

        AssertHasDiagnostic(result, "DP1201");
    }

    private static void AssertHasDiagnostic(
        GeneratorTestResult result,
        string diagnosticId)
    {
        Assert.That(
            result.GeneratorDiagnostics.Select(x => x.Id),
            Does.Contain(diagnosticId));
    }
}
