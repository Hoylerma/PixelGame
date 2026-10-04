using KettleCog.Simulation;
using UnityEngine;

namespace KettleCog.Content
{
    [CreateAssetMenu(menuName = "Kettle & Cog/Building", fileName = "building_")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [Tooltip("Stable id, e.g. building.malt_house.")]
        [SerializeField] string id = "building.";
        [SerializeField] string displayName;
        [SerializeField, TextArea] string description;
        [Tooltip("Footprint in tiles when facing North.")]
        [SerializeField] Vector2Int footprint = new Vector2Int(2, 2);
        [SerializeField, Range(0, 5)] int tier;
        [SerializeField, Min(0)] int costCoins;
        [Tooltip("0 for unpowered buildings.")]
        [SerializeField, Min(0f)] float powerDrawKw;

        [Header("Crafting")]
        [Tooltip("Recipe category this building crafts, e.g. malting. Leave empty for storage, belts and decoration.")]
        [SerializeField] string recipeCategory;
        [SerializeField, Min(0.01f)] float craftingSpeed = 1f;
        [SerializeField, Min(1)] int inputSlots = 4;
        [SerializeField, Min(1)] int outputSlots = 4;

        [Header("Presentation")]
        [Tooltip("Pivot at the bottom-centre of the footprint; sprite one tile taller than the footprint for the roof.")]
        [SerializeField] GameObject prefab;
        [SerializeField] Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;

        public BuildingDef Bake() => new BuildingDef(
            id,
            footprint.x,
            footprint.y,
            tier,
            costCoins,
            powerDrawKw,
            craftingSpeed,
            recipeCategory,
            inputSlots,
            outputSlots);
    }
}
