using NUnit.Framework;

namespace DynamicProperty.SourceGen.Tests;

[TestFixture]
public sealed class PropertyStorageIdTests
{
    [Test]
    public void EncodeSeparatesLogicalIdsAndSlots()
    {
        Assert.That(Encode(1, 0), Is.Not.EqualTo(Encode(2, 0)));
        Assert.That(Encode(6, 0), Is.Not.EqualTo(Encode(6, 1)));
        Assert.That(Encode(6, 1), Is.Not.EqualTo(Encode(6, 2)));
        Assert.That(Encode(6, 0), Is.EqualTo((6 << 3) | 0));
        Assert.That(Encode(6, 2), Is.EqualTo((6 << 3) | 2));
    }

    [Test]
    public void BoundaryLogicalIdWorks()
    {
        Assert.That(
            Encode(MaxLogicalId, 7),
            Is.EqualTo((MaxLogicalId << 3) | 7));
    }

    [TestCase(0, 0)]
    [TestCase(-1, 0)]
    [TestCase(MaxLogicalId + 1, 0)]
    [TestCase(1, -1)]
    [TestCase(1, 8)]
    public void InvalidInputsFailExplicitly(int logicalId, int slot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Encode(logicalId, slot));
    }

    private const int MaxLogicalId = int.MaxValue >> 3;

    private static int Encode(int logicalId, int slot)
    {
        if (logicalId <= 0 || logicalId > MaxLogicalId)
            throw new ArgumentOutOfRangeException(nameof(logicalId));

        if (slot < 0 || slot > 7)
            throw new ArgumentOutOfRangeException(nameof(slot));

        return (logicalId << 3) | slot;
    }
}
