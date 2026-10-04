using KettleCog.Simulation;
using UnityEngine;

namespace KettleCog.Content
{
    [CreateAssetMenu(menuName = "Kettle & Cog/Item", fileName = "item_")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Tooltip("Stable id used by saves, recipes and mods, e.g. item.pale_malt. Never change it after release.")]
        [SerializeField] string id = "item.";
        [SerializeField] string displayName;
        [SerializeField, TextArea] string description;
        [SerializeField] ItemKind kind = ItemKind.Solid;
        [Tooltip("Units per slot; litres per buffer slot for fluids.")]
        [SerializeField, Min(1)] int stackSize = 50;
        [Tooltip("Coins per unit (per litre for fluids) before quality and market modifiers.")]
        [SerializeField, Min(0)] int baseValue = 1;

        [Header("Art")]
        [Tooltip("16×16 UI icon.")]
        [SerializeField] Sprite icon;
        [Tooltip("8×8 sprite drawn on belts. Never downscale the icon instead.")]
        [SerializeField] Sprite beltSprite;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite BeltSprite => beltSprite;

        public ItemDef Bake() => new ItemDef(id, kind, stackSize, baseValue);
    }
}
