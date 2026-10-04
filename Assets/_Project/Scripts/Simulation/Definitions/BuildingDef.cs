using System;
using KettleCog.Core;

namespace KettleCog.Simulation
{
    /// <summary>Immutable runtime building definition, baked from a BuildingDefinition asset at boot.</summary>
    public sealed class BuildingDef
    {
        public BuildingDef(
            string id,
            int width,
            int height,
            int tier,
            long costCoins,
            float powerDrawKw = 0f,
            float craftingSpeed = 1f,
            string recipeCategory = null,
            int inputSlots = 4,
            int outputSlots = 4)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Building id must not be empty.", nameof(id));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width), $"Building '{id}' needs a positive footprint.");
            if (costCoins < 0) throw new ArgumentOutOfRangeException(nameof(costCoins), costCoins, "Cost must not be negative.");
            if (powerDrawKw < 0f) throw new ArgumentOutOfRangeException(nameof(powerDrawKw), powerDrawKw, "Power draw must not be negative.");
            if (craftingSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(craftingSpeed), craftingSpeed, "Crafting speed must be positive.");
            if (inputSlots <= 0 || outputSlots <= 0) throw new ArgumentOutOfRangeException(nameof(inputSlots), "Buffers need at least one slot.");

            Id = id;
            Width = width;
            Height = height;
            Tier = tier;
            CostCoins = costCoins;
            PowerDrawKw = powerDrawKw;
            CraftingSpeed = craftingSpeed;
            RecipeCategory = string.IsNullOrWhiteSpace(recipeCategory) ? null : recipeCategory;
            InputSlots = inputSlots;
            OutputSlots = outputSlots;
        }

        /// <summary>Stable content id, e.g. <c>building.malt_house</c>.</summary>
        public string Id { get; }

        /// <summary>Footprint in tiles when facing North or South.</summary>
        public int Width { get; }
        public int Height { get; }

        public int Tier { get; }
        public long CostCoins { get; }

        /// <summary>0 for unpowered buildings, which always run at full speed.</summary>
        public float PowerDrawKw { get; }

        public float CraftingSpeed { get; }

        /// <summary>Recipe category this building crafts, or null if it is not a crafter.</summary>
        public string RecipeCategory { get; }

        public int InputSlots { get; }
        public int OutputSlots { get; }

        public bool IsCrafter => RecipeCategory != null;
        public bool NeedsPower => PowerDrawKw > 0f;

        /// <summary>Sprites never rotate (¾ view); rotation only swaps the footprint and moves the ports.</summary>
        public (int Width, int Height) FootprintFor(Direction facing) =>
            facing.IsHorizontal() ? (Height, Width) : (Width, Height);

        public override string ToString() => Id;
    }
}
