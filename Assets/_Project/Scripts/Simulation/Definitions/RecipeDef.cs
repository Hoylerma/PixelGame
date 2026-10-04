using System;
using System.Collections.Generic;

namespace KettleCog.Simulation
{
    /// <summary>Immutable runtime recipe: inputs become outputs after a fixed number of ticks at speed 1.0.</summary>
    public sealed class RecipeDef
    {
        public RecipeDef(string id, string category, ItemAmount[] inputs, ItemAmount[] outputs, int durationTicks)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Recipe id must not be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException($"Recipe '{id}' needs a category.", nameof(category));
            if (outputs == null || outputs.Length == 0) throw new ArgumentException($"Recipe '{id}' needs at least one output.", nameof(outputs));
            if (durationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(durationTicks), durationTicks, $"Recipe '{id}' needs a positive duration.");

            Id = id;
            Category = category;
            Inputs = CopyWithoutDuplicates(id, inputs ?? Array.Empty<ItemAmount>());
            Outputs = CopyWithoutDuplicates(id, outputs);
            DurationTicks = durationTicks;
        }

        /// <summary>Stable content id, e.g. <c>recipe.pale_malt</c>.</summary>
        public string Id { get; }

        /// <summary>Which buildings may craft this, e.g. <c>malting</c>. Matches <see cref="BuildingDef.RecipeCategory"/>.</summary>
        public string Category { get; }

        public IReadOnlyList<ItemAmount> Inputs { get; }
        public IReadOnlyList<ItemAmount> Outputs { get; }
        public int DurationTicks { get; }

        public float DurationSeconds => DurationTicks / (float)SimulationClock.TicksPerSecond;

        public static int SecondsToTicks(float seconds) =>
            Math.Max(1, (int)Math.Round(seconds * SimulationClock.TicksPerSecond));

        public override string ToString() => Id;

        static ItemAmount[] CopyWithoutDuplicates(string recipeId, ItemAmount[] amounts)
        {
            var copy = new ItemAmount[amounts.Length];
            for (int i = 0; i < amounts.Length; i++)
            {
                if (amounts[i].Item == null)
                    throw new ArgumentException($"Recipe '{recipeId}' has an empty item entry.");
                for (int j = 0; j < i; j++)
                {
                    if (ReferenceEquals(copy[j].Item, amounts[i].Item))
                        throw new ArgumentException($"Recipe '{recipeId}' lists '{amounts[i].Item.Id}' twice; merge the amounts instead.");
                }
                copy[i] = amounts[i];
            }
            return copy;
        }
    }
}
