using System;
using System.Collections.Generic;

namespace KettleCog.Simulation
{
    public struct ItemStack
    {
        public ItemDef Item;
        public int Count;

        public bool IsEmpty => Item == null || Count == 0;
    }

    /// <summary>
    /// Fixed number of slots, each holding one stack of a single item up to its stack size.
    /// Used for machine buffers, crates and the player's backpack.
    /// </summary>
    public sealed class Inventory
    {
        readonly ItemStack[] slots;

        public Inventory(int slotCount)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount), slotCount, "An inventory needs at least one slot.");
            slots = new ItemStack[slotCount];
        }

        public int SlotCount => slots.Length;
        public ItemStack this[int index] => slots[index];

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (!slots[i].IsEmpty) return false;
                }
                return true;
            }
        }

        public int Count(ItemDef item)
        {
            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (ReferenceEquals(slots[i].Item, item)) total += slots[i].Count;
            }
            return total;
        }

        public bool ContainsAll(IReadOnlyList<ItemAmount> amounts)
        {
            for (int i = 0; i < amounts.Count; i++)
            {
                if (Count(amounts[i].Item) < amounts[i].Amount) return false;
            }
            return true;
        }

        /// <summary>How many units of <paramref name="item"/> would fit right now.</summary>
        public int SpaceFor(ItemDef item) => SpaceInStacksOf(item) + EmptySlotCount() * item.StackSize;

        /// <summary>Whether every amount fits at once. Different items compete for the same empty slots.</summary>
        public bool CanInsertAll(IReadOnlyList<ItemAmount> amounts)
        {
            int freeSlots = EmptySlotCount();
            for (int i = 0; i < amounts.Count; i++)
            {
                var item = amounts[i].Item;
                int overflow = amounts[i].Amount - SpaceInStacksOf(item);
                if (overflow <= 0) continue;

                freeSlots -= (overflow + item.StackSize - 1) / item.StackSize;
                if (freeSlots < 0) return false;
            }
            return true;
        }

        /// <summary>Inserts as much as fits, topping up existing stacks first. Returns the amount inserted.</summary>
        public int Insert(ItemDef item, int amount)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must not be negative.");

            int remaining = amount;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!ReferenceEquals(slots[i].Item, item) || slots[i].Count == 0) continue;
                int moved = Math.Min(remaining, item.StackSize - slots[i].Count);
                slots[i].Count += moved;
                remaining -= moved;
            }
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;
                int moved = Math.Min(remaining, item.StackSize);
                slots[i] = new ItemStack { Item = item, Count = moved };
                remaining -= moved;
            }
            return amount - remaining;
        }

        /// <summary>Removes up to <paramref name="amount"/> units. Returns the amount removed.</summary>
        public int Remove(ItemDef item, int amount)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must not be negative.");

            int remaining = amount;
            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (!ReferenceEquals(slots[i].Item, item)) continue;
                int moved = Math.Min(remaining, slots[i].Count);
                slots[i].Count -= moved;
                remaining -= moved;
                if (slots[i].Count == 0) slots[i] = default;
            }
            return amount - remaining;
        }

        int SpaceInStacksOf(ItemDef item)
        {
            int space = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (ReferenceEquals(slots[i].Item, item) && slots[i].Count > 0) space += item.StackSize - slots[i].Count;
            }
            return space;
        }

        int EmptySlotCount()
        {
            int empty = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) empty++;
            }
            return empty;
        }
    }
}
