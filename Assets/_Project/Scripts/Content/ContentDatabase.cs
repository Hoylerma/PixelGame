using System;
using System.Collections.Generic;
using KettleCog.Simulation;
using UnityEngine;

namespace KettleCog.Content
{
    /// <summary>
    /// The list of all authoring assets. At boot it bakes them into an immutable <see cref="ContentRegistry"/>;
    /// the simulation never touches a ScriptableObject (GDD §3.7).
    /// </summary>
    [CreateAssetMenu(menuName = "Kettle & Cog/Content Database", fileName = "ContentDatabase")]
    public sealed class ContentDatabase : ScriptableObject
    {
        [SerializeField] List<ItemDefinition> items = new List<ItemDefinition>();
        [SerializeField] List<RecipeDefinition> recipes = new List<RecipeDefinition>();
        [SerializeField] List<BuildingDefinition> buildings = new List<BuildingDefinition>();

        public IReadOnlyList<ItemDefinition> Items => items;
        public IReadOnlyList<RecipeDefinition> Recipes => recipes;
        public IReadOnlyList<BuildingDefinition> Buildings => buildings;

        public ContentRegistry BuildRegistry()
        {
            var registry = new ContentRegistry();
            foreach (var item in items) registry.Register(Require(item, "item").Bake());
            foreach (var recipe in recipes) registry.Register(Require(recipe, "recipe").Bake(registry));
            foreach (var building in buildings) registry.Register(Require(building, "building").Bake());
            return registry;
        }

        T Require<T>(T asset, string kind) where T : UnityEngine.Object
        {
            if (asset == null) throw new InvalidOperationException($"{name} has an empty {kind} entry.");
            return asset;
        }
    }
}
