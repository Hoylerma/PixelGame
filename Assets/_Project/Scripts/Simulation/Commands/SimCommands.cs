using KettleCog.Core;

namespace KettleCog.Simulation
{
    /// <summary>
    /// A player action, applied at the start of the next tick. Commands reference content by string id and
    /// buildings by entity id, so they can be recorded for undo, replays, bug reports and lockstep co-op.
    /// </summary>
    public interface ISimCommand
    {
        /// <returns>False if the command was rejected (the world is unchanged).</returns>
        bool Execute(SimulationWorld world);
    }

    public sealed class PlaceBuildingCommand : ISimCommand
    {
        public PlaceBuildingCommand(string buildingId, GridPos origin, Direction facing = Direction.North)
        {
            BuildingId = buildingId;
            Origin = origin;
            Facing = facing;
        }

        public string BuildingId { get; }
        public GridPos Origin { get; }
        public Direction Facing { get; }

        public bool Execute(SimulationWorld world) =>
            world.Content.TryGetBuilding(BuildingId, out var def) &&
            world.TryPlaceBuilding(def, Origin, Facing, out _);
    }

    public sealed class RemoveBuildingCommand : ISimCommand
    {
        public RemoveBuildingCommand(int buildingId) => BuildingId = buildingId;

        public int BuildingId { get; }

        public bool Execute(SimulationWorld world) => world.TryRemoveBuilding(BuildingId);
    }

    public sealed class SetRecipeCommand : ISimCommand
    {
        /// <param name="recipeId">Null clears the recipe.</param>
        public SetRecipeCommand(int buildingId, string recipeId)
        {
            BuildingId = buildingId;
            RecipeId = recipeId;
        }

        public int BuildingId { get; }
        public string RecipeId { get; }

        public bool Execute(SimulationWorld world)
        {
            if (!world.TryGetBuilding(BuildingId, out var building) || building.Crafter == null) return false;

            if (RecipeId == null)
            {
                building.Crafter.SetRecipe(null);
                return true;
            }

            if (!world.Content.TryGetRecipe(RecipeId, out var recipe) || recipe.Category != building.Def.RecipeCategory) return false;
            building.Crafter.SetRecipe(recipe);
            return true;
        }
    }
}
