using System;
using System.Collections.Generic;
using KettleCog.Simulation;
using UnityEngine;

namespace KettleCog.Content
{
    [Serializable]
    public struct ItemAmountEntry
    {
        public ItemDefinition item;
        [Min(1)] public int amount;
    }

    [CreateAssetMenu(menuName = "Kettle & Cog/Recipe", fileName = "recipe_")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Tooltip("Stable id, e.g. recipe.pale_malt.")]
        [SerializeField] string id = "recipe.";
        [SerializeField] string displayName;
        [Tooltip("Must match BuildingDefinition.recipeCategory of the buildings that craft it, e.g. malting.")]
        [SerializeField] string category;
        [Tooltip("Real seconds per cycle at crafting speed 1.0.")]
        [SerializeField, Min(0.05f)] float durationSeconds = 60f;
        [SerializeField] List<ItemAmountEntry> inputs = new List<ItemAmountEntry>();
        [SerializeField] List<ItemAmountEntry> outputs = new List<ItemAmountEntry>();

        public string Id => id;
        public string DisplayName => displayName;

        /// <summary>Items must already be registered; <see cref="ContentDatabase"/> bakes items first.</summary>
        public RecipeDef Bake(ContentRegistry registry) =>
            new RecipeDef(id, category, Resolve(inputs, registry), Resolve(outputs, registry), RecipeDef.SecondsToTicks(durationSeconds));

        ItemAmount[] Resolve(List<ItemAmountEntry> entries, ContentRegistry registry)
        {
            var amounts = new ItemAmount[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.item == null) throw new InvalidOperationException($"Recipe '{id}' has an empty item slot.");
                amounts[i] = new ItemAmount(registry.GetItem(entry.item.Id), entry.amount);
            }
            return amounts;
        }
    }
}
