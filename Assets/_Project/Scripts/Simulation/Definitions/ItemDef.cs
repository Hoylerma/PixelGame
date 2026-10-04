using System;

namespace KettleCog.Simulation
{
    public enum ItemKind : byte
    {
        Solid,
        Fluid,
    }

    /// <summary>
    /// Immutable runtime item definition, baked from an ItemDefinition asset at boot.
    /// Fluids use litres as their unit, so their stack size is the litres one buffer slot holds.
    /// </summary>
    public sealed class ItemDef
    {
        public ItemDef(string id, ItemKind kind, int stackSize, int baseValue)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Item id must not be empty.", nameof(id));
            if (stackSize <= 0) throw new ArgumentOutOfRangeException(nameof(stackSize), stackSize, "Stack size must be positive.");
            if (baseValue < 0) throw new ArgumentOutOfRangeException(nameof(baseValue), baseValue, "Base value must not be negative.");

            Id = id;
            Kind = kind;
            StackSize = stackSize;
            BaseValue = baseValue;
        }

        /// <summary>Stable content id used by saves and mods, e.g. <c>item.pale_malt</c>.</summary>
        public string Id { get; }

        /// <summary>Dense runtime index assigned by <see cref="ContentRegistry"/>; -1 until registered.</summary>
        public int Index { get; internal set; } = -1;

        public ItemKind Kind { get; }
        public int StackSize { get; }

        /// <summary>Coins per unit (per litre for fluids) before quality and market modifiers.</summary>
        public int BaseValue { get; }

        public override string ToString() => Id;
    }

    public readonly struct ItemAmount
    {
        public readonly ItemDef Item;
        public readonly int Amount;

        public ItemAmount(ItemDef item, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Amount = amount;
        }

        public override string ToString() => $"{Amount}× {Item.Id}";
    }
}
