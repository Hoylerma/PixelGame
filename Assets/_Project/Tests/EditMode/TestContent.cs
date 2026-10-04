using KettleCog.Simulation;

namespace KettleCog.Tests
{
    /// <summary>A tiny slice of the Tier 0–1 content (GDD §4.3) for simulation tests.</summary>
    sealed class TestContent
    {
        public const string MaltHouseId = "building.malt_house";
        public const string ElectricMaltHouseId = "building.malt_house_mk2";
        public const string CrateId = "building.crate";
        public const string PaleMaltRecipeId = "recipe.pale_malt";

        public readonly ContentRegistry Registry = new ContentRegistry();
        public readonly ItemDef Barley = new ItemDef("item.barley", ItemKind.Solid, stackSize: 50, baseValue: 1);
        public readonly ItemDef Water = new ItemDef("item.water", ItemKind.Fluid, stackSize: 1000, baseValue: 0);
        public readonly ItemDef PaleMalt = new ItemDef("item.pale_malt", ItemKind.Solid, stackSize: 50, baseValue: 3);
        public readonly RecipeDef PaleMaltRecipe;
        public readonly BuildingDef MaltHouse;
        public readonly BuildingDef ElectricMaltHouse;
        public readonly BuildingDef Crate;

        public TestContent()
        {
            Registry.Register(Barley);
            Registry.Register(Water);
            Registry.Register(PaleMalt);

            // 10 Barley + 50 L Water → 10 Pale Malt / 60 s.
            PaleMaltRecipe = new RecipeDef(
                PaleMaltRecipeId,
                "malting",
                new[] { new ItemAmount(Barley, 10), new ItemAmount(Water, 50) },
                new[] { new ItemAmount(PaleMalt, 10) },
                RecipeDef.SecondsToTicks(60f));
            Registry.Register(PaleMaltRecipe);

            MaltHouse = new BuildingDef(MaltHouseId, width: 3, height: 3, tier: 1, costCoins: 400, recipeCategory: "malting");
            ElectricMaltHouse = new BuildingDef(ElectricMaltHouseId, width: 3, height: 3, tier: 2, costCoins: 2500,
                powerDrawKw: 30f, craftingSpeed: 2f, recipeCategory: "malting");
            Crate = new BuildingDef(CrateId, width: 1, height: 1, tier: 0, costCoins: 30);
            Registry.Register(MaltHouse);
            Registry.Register(ElectricMaltHouse);
            Registry.Register(Crate);
        }

        public Crafter NewMaltHouseCrafter(BuildingDef building = null)
        {
            var crafter = new Crafter(building ?? MaltHouse);
            crafter.SetRecipe(PaleMaltRecipe);
            return crafter;
        }

        public void FeedOneBatch(Crafter crafter)
        {
            crafter.Input.Insert(Barley, 10);
            crafter.Input.Insert(Water, 50);
        }
    }
}
