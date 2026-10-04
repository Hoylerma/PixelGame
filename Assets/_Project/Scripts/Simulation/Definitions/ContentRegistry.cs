using System;
using System.Collections.Generic;

namespace KettleCog.Simulation
{
    /// <summary>
    /// All runtime definitions, looked up by stable string id or dense item index.
    /// Register items first: recipes may only reference items already registered here.
    /// </summary>
    public sealed class ContentRegistry
    {
        readonly Dictionary<string, ItemDef> items = new Dictionary<string, ItemDef>(StringComparer.Ordinal);
        readonly List<ItemDef> itemsByIndex = new List<ItemDef>();
        readonly Dictionary<string, RecipeDef> recipes = new Dictionary<string, RecipeDef>(StringComparer.Ordinal);
        readonly Dictionary<string, BuildingDef> buildings = new Dictionary<string, BuildingDef>(StringComparer.Ordinal);

        public IReadOnlyList<ItemDef> Items => itemsByIndex;
        public IEnumerable<RecipeDef> Recipes => recipes.Values;
        public IEnumerable<BuildingDef> Buildings => buildings.Values;

        public void Register(ItemDef item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Index >= 0) throw new InvalidOperationException($"Item '{item.Id}' is already registered in a registry.");
            EnsureUnique(items, item.Id, "item");

            item.Index = itemsByIndex.Count;
            itemsByIndex.Add(item);
            items.Add(item.Id, item);
        }

        public void Register(RecipeDef recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            EnsureUnique(recipes, recipe.Id, "recipe");
            EnsureItemsRegistered(recipe, recipe.Inputs);
            EnsureItemsRegistered(recipe, recipe.Outputs);
            recipes.Add(recipe.Id, recipe);
        }

        public void Register(BuildingDef building)
        {
            if (building == null) throw new ArgumentNullException(nameof(building));
            EnsureUnique(buildings, building.Id, "building");
            buildings.Add(building.Id, building);
        }

        public ItemDef GetItem(int index) => itemsByIndex[index];
        public ItemDef GetItem(string id) => Get(items, id, "item");
        public RecipeDef GetRecipe(string id) => Get(recipes, id, "recipe");
        public BuildingDef GetBuilding(string id) => Get(buildings, id, "building");

        public bool TryGetItem(string id, out ItemDef item) => items.TryGetValue(id, out item);
        public bool TryGetRecipe(string id, out RecipeDef recipe) => recipes.TryGetValue(id, out recipe);
        public bool TryGetBuilding(string id, out BuildingDef building) => buildings.TryGetValue(id, out building);

        void EnsureItemsRegistered(RecipeDef recipe, IReadOnlyList<ItemAmount> amounts)
        {
            for (int i = 0; i < amounts.Count; i++)
            {
                var item = amounts[i].Item;
                if (!items.TryGetValue(item.Id, out var registered) || !ReferenceEquals(registered, item))
                    throw new InvalidOperationException($"Recipe '{recipe.Id}' uses item '{item.Id}', which is not registered here.");
            }
        }

        static void EnsureUnique<T>(Dictionary<string, T> map, string id, string kind)
        {
            if (map.ContainsKey(id)) throw new InvalidOperationException($"Duplicate {kind} id '{id}'.");
        }

        static T Get<T>(Dictionary<string, T> map, string id, string kind)
        {
            if (!map.TryGetValue(id, out var value)) throw new KeyNotFoundException($"Unknown {kind} id '{id}'.");
            return value;
        }
    }
}
