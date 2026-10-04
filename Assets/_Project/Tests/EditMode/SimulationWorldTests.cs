using KettleCog.Core;
using KettleCog.Simulation;
using NUnit.Framework;

namespace KettleCog.Tests
{
    public class SimulationWorldTests
    {
        TestContent content;
        SimulationWorld world;

        [SetUp]
        public void SetUp()
        {
            content = new TestContent();
            world = new SimulationWorld(content.Registry, startingCoins: 1000);
        }

        [Test]
        public void PlacingABuildingCostsCoinsAndOccupiesItsFootprint()
        {
            world.Enqueue(new PlaceBuildingCommand(TestContent.MaltHouseId, new GridPos(0, 0)));
            world.Step();

            Assert.That(world.Wallet.Coins, Is.EqualTo(600));
            Assert.That(world.Grid.OccupiedTileCount, Is.EqualTo(9));
            Assert.That(world.TryGetBuildingAt(new GridPos(2, 2), out var building), Is.True);
            Assert.That(building.Def, Is.SameAs(content.MaltHouse));
        }

        [Test]
        public void RejectsOverlapsAndUnaffordableBuildings()
        {
            ISimCommand rejected = null;
            world.CommandRejected += command => rejected = command;

            world.Enqueue(new PlaceBuildingCommand(TestContent.MaltHouseId, new GridPos(0, 0)));
            world.Enqueue(new PlaceBuildingCommand(TestContent.CrateId, new GridPos(1, 1)));
            world.Step();
            Assert.That(rejected, Is.InstanceOf<PlaceBuildingCommand>());

            world.Enqueue(new PlaceBuildingCommand(TestContent.ElectricMaltHouseId, new GridPos(10, 0)));
            world.Step();
            Assert.That(world.Buildings.Count, Is.EqualTo(1));
            Assert.That(world.Wallet.Coins, Is.EqualTo(600));
        }

        [Test]
        public void RotationSwapsFootprint()
        {
            var wide = new BuildingDef("building.bottling_line", width: 4, height: 2, tier: 2, costCoins: 0);
            content.Registry.Register(wide);

            world.Enqueue(new PlaceBuildingCommand(wide.Id, new GridPos(0, 0), Direction.East));
            world.Step();

            Assert.That(world.Grid.IsOccupied(new GridPos(1, 3)), Is.True);
            Assert.That(world.Grid.IsOccupied(new GridPos(2, 0)), Is.False);
        }

        [Test]
        public void RemovingABuildingRefundsAndFreesTiles()
        {
            world.Enqueue(new PlaceBuildingCommand(TestContent.MaltHouseId, new GridPos(0, 0)));
            world.Step();
            world.TryGetBuildingAt(GridPos.Zero, out var building);

            world.Enqueue(new RemoveBuildingCommand(building.Id));
            world.Step();

            Assert.That(world.Wallet.Coins, Is.EqualTo(1000));
            Assert.That(world.Grid.OccupiedTileCount, Is.Zero);
            Assert.That(world.Crafting.Crafters, Is.Empty);
        }

        [Test]
        public void PlacedCrafterRunsInsideTheTickPipeline()
        {
            world.Enqueue(new PlaceBuildingCommand(TestContent.MaltHouseId, new GridPos(0, 0)));
            world.Step();
            world.TryGetBuildingAt(GridPos.Zero, out var building);

            world.Enqueue(new SetRecipeCommand(building.Id, TestContent.PaleMaltRecipeId));
            content.FeedOneBatch(building.Crafter);
            for (int i = 0; i < 60 * SimulationClock.TicksPerSecond + 1; i++) world.Step();

            Assert.That(building.Crafter.Output.Count(content.PaleMalt), Is.EqualTo(10));
        }

        [Test]
        public void RejectsRecipeFromAnotherCategory()
        {
            var otherRecipe = new RecipeDef("recipe.planks", "sawing",
                new[] { new ItemAmount(content.Barley, 1) }, new[] { new ItemAmount(content.PaleMalt, 1) }, 20);
            content.Registry.Register(otherRecipe);
            world.Enqueue(new PlaceBuildingCommand(TestContent.MaltHouseId, new GridPos(0, 0)));
            world.Step();
            world.TryGetBuildingAt(GridPos.Zero, out var building);

            Assert.That(new SetRecipeCommand(building.Id, otherRecipe.Id).Execute(world), Is.False);
            Assert.That(building.Crafter.Recipe, Is.Null);
        }
    }
}
