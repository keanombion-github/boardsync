namespace Boardsync.Api.Common.Ordering;

public readonly record struct PositionedItem(Guid Id, double Position);

public static class FractionalPosition
{
    public static bool TryCalculate(
        IReadOnlyList<PositionedItem> orderedItems,
        Guid? beforeId,
        Guid? afterId,
        out double position)
    {
        position = default;

        if (orderedItems.Count == 0)
        {
            if (beforeId.HasValue || afterId.HasValue)
                return false;

            position = 1.0;
            return true;
        }

        if (!beforeId.HasValue)
        {
            if (afterId != orderedItems[0].Id)
                return false;

            position = orderedItems[0].Position / 2.0;
            return double.IsFinite(position)
                && position > 0
                && position < orderedItems[0].Position;
        }

        if (!afterId.HasValue)
        {
            var lastItem = orderedItems[^1];

            if (beforeId != lastItem.Id)
                return false;

            position = lastItem.Position + 1.0;
            return double.IsFinite(position)
                && position > lastItem.Position;
        }

        var beforeIndex = FindIndex(orderedItems, beforeId.Value);
        var afterIndex = FindIndex(orderedItems, afterId.Value);

        if (beforeIndex < 0 || afterIndex != beforeIndex + 1)
            return false;

        if (orderedItems[beforeIndex].Position
            >= orderedItems[afterIndex].Position)
        {
            return false;
        }

        position = (
            orderedItems[beforeIndex].Position
            + orderedItems[afterIndex].Position
        ) / 2.0;

        return double.IsFinite(position)
            && position > orderedItems[beforeIndex].Position
            && position < orderedItems[afterIndex].Position;
    }

    private static int FindIndex(
        IReadOnlyList<PositionedItem> orderedItems,
        Guid id)
    {
        for (var index = 0; index < orderedItems.Count; index++)
        {
            if (orderedItems[index].Id == id)
                return index;
        }

        return -1;
    }
}
