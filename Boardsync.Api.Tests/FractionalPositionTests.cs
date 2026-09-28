using Boardsync.Api.Common.Ordering;

namespace Boardsync.Api.Tests;

public class FractionalPositionTests
{
    private static readonly Guid FirstId = Guid.Parse(
        "00000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondId = Guid.Parse(
        "00000000-0000-0000-0000-000000000002");
    private static readonly Guid ThirdId = Guid.Parse(
        "00000000-0000-0000-0000-000000000003");

    private static readonly PositionedItem[] Items =
    [
        new(FirstId, 1.0),
        new(SecondId, 2.0),
        new(ThirdId, 3.0)
    ];

    [Fact]
    public void EmptyList_WithNoNeighbors_StartsAtOne()
    {
        var valid = FractionalPosition.TryCalculate(
            [],
            null,
            null,
            out var position);

        Assert.True(valid);
        Assert.Equal(1.0, position);
    }

    [Fact]
    public void OccupiedList_WithNoNeighbors_IsRejected()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            null,
            null,
            out _);

        Assert.False(valid);
    }

    [Fact]
    public void FirstItem_AsAfterNeighbor_InsertsAtTop()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            null,
            FirstId,
            out var position);

        Assert.True(valid);
        Assert.Equal(0.5, position);
    }

    [Fact]
    public void LastItem_AsBeforeNeighbor_AppendsAtBottom()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            ThirdId,
            null,
            out var position);

        Assert.True(valid);
        Assert.Equal(4.0, position);
    }

    [Fact]
    public void AdjacentNeighbors_InsertAtMidpoint()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            FirstId,
            SecondId,
            out var position);

        Assert.True(valid);
        Assert.Equal(1.5, position);
    }

    [Fact]
    public void NonAdjacentNeighbors_AreRejected()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            FirstId,
            ThirdId,
            out _);

        Assert.False(valid);
    }

    [Fact]
    public void NeighborsWithEqualPositions_AreRejected()
    {
        PositionedItem[] duplicatePositions =
        [
            new(FirstId, 1.0),
            new(SecondId, 1.0)
        ];

        var valid = FractionalPosition.TryCalculate(
            duplicatePositions,
            FirstId,
            SecondId,
            out _);

        Assert.False(valid);
    }

    [Fact]
    public void UnknownNeighbor_IsRejected()
    {
        var valid = FractionalPosition.TryCalculate(
            Items,
            Guid.NewGuid(),
            null,
            out _);

        Assert.False(valid);
    }
}
