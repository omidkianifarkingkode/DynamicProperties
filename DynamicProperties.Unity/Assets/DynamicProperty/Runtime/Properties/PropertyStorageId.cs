using System;
using System.ComponentModel;

namespace DynamicProperty
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PropertyStorageId
    {
        public const int SlotBits = 3;
        public const int SlotCount = 1 << SlotBits;
        public const int MaxSlot = SlotCount - 1;
        public const int MaxLogicalId = int.MaxValue >> SlotBits;

        public static int Encode(int logicalId, int slot)
        {
            if (logicalId <= 0 || logicalId > MaxLogicalId)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(logicalId),
                    logicalId,
                    $"DynamicProperty logical IDs must be between 1 and {MaxLogicalId}.");
            }

            if (slot < 0 || slot > MaxSlot)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slot),
                    slot,
                    $"DynamicProperty storage slots must be between 0 and {MaxSlot}.");
            }

            return (logicalId << SlotBits) | slot;
        }

        public static bool TryDecode(int storageId, out int logicalId, out int slot)
        {
            logicalId = storageId >> SlotBits;
            slot = storageId & MaxSlot;

            return logicalId > 0 && logicalId <= MaxLogicalId;
        }
    }
}
