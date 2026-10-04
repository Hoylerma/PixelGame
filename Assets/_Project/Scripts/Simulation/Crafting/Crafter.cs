using System;

namespace KettleCog.Simulation
{
    public enum CrafterStatus : byte
    {
        NoRecipe,
        MissingInputs,
        Working,
        OutputBlocked,
        NoPower,
    }

    /// <summary>
    /// Recipe processor inside a building (Malt House, Mash Tun, Fermenter, ...).
    /// A cycle starts once all inputs are present, and inputs are consumed when the cycle completes,
    /// so changing the recipe mid-cycle never loses items. A finished cycle waits until its outputs fit.
    /// </summary>
    public sealed class Crafter
    {
        /// <summary>Feeders keep this many crafts' worth of each input buffered.</summary>
        public const int InputBufferCrafts = 2;

        float progressTicks;
        bool cycleActive;

        public Crafter(BuildingDef building)
        {
            Building = building ?? throw new ArgumentNullException(nameof(building));
            if (!building.IsCrafter) throw new ArgumentException($"Building '{building.Id}' has no recipe category.", nameof(building));

            Input = new Inventory(building.InputSlots);
            Output = new Inventory(building.OutputSlots);
        }

        public BuildingDef Building { get; }
        public RecipeDef Recipe { get; private set; }
        public Inventory Input { get; }
        public Inventory Output { get; }
        public CrafterStatus Status { get; private set; } = CrafterStatus.NoRecipe;
        public int CompletedCycles { get; private set; }

        /// <summary>0..1 progress of the current cycle, for UI bars and working animations.</summary>
        public float Progress => Recipe == null ? 0f : Math.Min(1f, progressTicks / Recipe.DurationTicks);

        public void SetRecipe(RecipeDef recipe)
        {
            if (recipe != null && recipe.Category != Building.RecipeCategory)
                throw new ArgumentException($"'{Building.Id}' cannot craft '{recipe.Id}' (category '{recipe.Category}').", nameof(recipe));

            Recipe = recipe;
            cycleActive = false;
            progressTicks = 0f;
            Status = recipe == null ? CrafterStatus.NoRecipe : CrafterStatus.MissingInputs;
        }

        /// <summary>How many more units of <paramref name="item"/> a grabber should insert.</summary>
        public int InputDemand(ItemDef item)
        {
            if (Recipe == null) return 0;
            for (int i = 0; i < Recipe.Inputs.Count; i++)
            {
                var input = Recipe.Inputs[i];
                if (ReferenceEquals(input.Item, item)) return Math.Max(0, input.Amount * InputBufferCrafts - Input.Count(item));
            }
            return 0;
        }

        /// <param name="powerSatisfaction">0..1 share of power demand met this tick; ignored by unpowered buildings.</param>
        public void Tick(float powerSatisfaction)
        {
            if (Recipe == null)
            {
                Status = CrafterStatus.NoRecipe;
                return;
            }

            if (!cycleActive && !TryStartCycle()) return;

            if (progressTicks < Recipe.DurationTicks)
            {
                float speed = EffectiveSpeed(powerSatisfaction);
                if (speed <= 0f)
                {
                    Status = CrafterStatus.NoPower;
                    return;
                }
                progressTicks += speed;
                Status = CrafterStatus.Working;
            }

            if (progressTicks >= Recipe.DurationTicks) TryFinishCycle();
        }

        float EffectiveSpeed(float powerSatisfaction)
        {
            if (!Building.NeedsPower) return Building.CraftingSpeed;
            float satisfaction = powerSatisfaction < 0f ? 0f : powerSatisfaction > 1f ? 1f : powerSatisfaction;
            return Building.CraftingSpeed * satisfaction;
        }

        bool TryStartCycle()
        {
            if (!Input.ContainsAll(Recipe.Inputs))
            {
                Status = CrafterStatus.MissingInputs;
                return false;
            }
            cycleActive = true;
            progressTicks = 0f;
            return true;
        }

        void TryFinishCycle()
        {
            if (!Output.CanInsertAll(Recipe.Outputs))
            {
                Status = CrafterStatus.OutputBlocked;
                return;
            }

            for (int i = 0; i < Recipe.Inputs.Count; i++) Input.Remove(Recipe.Inputs[i].Item, Recipe.Inputs[i].Amount);
            for (int i = 0; i < Recipe.Outputs.Count; i++) Output.Insert(Recipe.Outputs[i].Item, Recipe.Outputs[i].Amount);
            CompletedCycles++;

            // Carry leftover progress into the next cycle so non-integer speeds keep exact long-run rates.
            // At most one cycle completes per tick, so the carry is capped at one full cycle.
            float carry = Math.Min(progressTicks - Recipe.DurationTicks, Recipe.DurationTicks);
            cycleActive = false;
            progressTicks = 0f;
            if (TryStartCycle())
            {
                progressTicks = carry;
                Status = CrafterStatus.Working;
            }
        }
    }
}
