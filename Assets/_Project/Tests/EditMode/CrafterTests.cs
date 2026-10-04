using KettleCog.Simulation;
using NUnit.Framework;

namespace KettleCog.Tests
{
    public class CrafterTests
    {
        const int SixtySeconds = 60 * SimulationClock.TicksPerSecond;

        TestContent content;

        [SetUp]
        public void SetUp() => content = new TestContent();

        [Test]
        public void CompletesCycleAfterRecipeDuration()
        {
            var crafter = content.NewMaltHouseCrafter();
            content.FeedOneBatch(crafter);

            Tick(crafter, SixtySeconds - 1);
            Assert.That(crafter.Output.Count(content.PaleMalt), Is.Zero);
            Assert.That(crafter.Status, Is.EqualTo(CrafterStatus.Working));

            crafter.Tick(1f);
            Assert.That(crafter.Output.Count(content.PaleMalt), Is.EqualTo(10));
            Assert.That(crafter.Input.IsEmpty, Is.True);
            Assert.That(crafter.CompletedCycles, Is.EqualTo(1));
        }

        [Test]
        public void WaitsForMissingInputs()
        {
            var crafter = content.NewMaltHouseCrafter();
            crafter.Input.Insert(content.Barley, 10);

            Tick(crafter, SixtySeconds * 2);

            Assert.That(crafter.Status, Is.EqualTo(CrafterStatus.MissingInputs));
            Assert.That(crafter.Output.IsEmpty, Is.True);
            Assert.That(crafter.Input.Count(content.Barley), Is.EqualTo(10));
        }

        [Test]
        public void HoldsFinishedCycleWhileOutputIsBlocked()
        {
            var crafter = content.NewMaltHouseCrafter();
            for (int i = 0; i < crafter.Output.SlotCount; i++) crafter.Output.Insert(content.Barley, content.Barley.StackSize);
            content.FeedOneBatch(crafter);

            Tick(crafter, SixtySeconds + 10);
            Assert.That(crafter.Status, Is.EqualTo(CrafterStatus.OutputBlocked));
            Assert.That(crafter.Input.Count(content.Barley), Is.EqualTo(10), "inputs are only consumed once the output fits");

            crafter.Output.Remove(content.Barley, content.Barley.StackSize);
            crafter.Tick(1f);
            Assert.That(crafter.Output.Count(content.PaleMalt), Is.EqualTo(10));
        }

        [Test]
        public void PoweredBuildingSlowsDownInBrownout()
        {
            var crafter = content.NewMaltHouseCrafter(content.ElectricMaltHouse); // speed 2.0
            content.FeedOneBatch(crafter);

            Tick(crafter, SixtySeconds - 1, powerSatisfaction: 0.5f);
            Assert.That(crafter.CompletedCycles, Is.Zero);

            crafter.Tick(0.5f);
            Assert.That(crafter.CompletedCycles, Is.EqualTo(1));
        }

        [Test]
        public void PoweredBuildingStopsWithoutPower()
        {
            var crafter = content.NewMaltHouseCrafter(content.ElectricMaltHouse);
            content.FeedOneBatch(crafter);

            crafter.Tick(0f);

            Assert.That(crafter.Status, Is.EqualTo(CrafterStatus.NoPower));
            Assert.That(crafter.Progress, Is.Zero);
        }

        [Test]
        public void CarriesProgressSoFractionalSpeedsKeepExactRates()
        {
            var building = new BuildingDef("building.test_malter", 1, 1, 0, 0, craftingSpeed: 1.75f, recipeCategory: "malting");
            var crafter = content.NewMaltHouseCrafter(building);
            for (int i = 0; i < 7; i++) content.FeedOneBatch(crafter);

            // Seven cycles need exactly 7 × 1200 / 1.75 = 4800 ticks. Without the carry,
            // every cycle would round up to 686 ticks and the seventh would finish at 4802.
            Tick(crafter, 4799);
            Assert.That(crafter.CompletedCycles, Is.EqualTo(6));

            crafter.Tick(1f);
            Assert.That(crafter.CompletedCycles, Is.EqualTo(7));
        }

        [Test]
        public void ChangingRecipeKeepsBufferedInputs()
        {
            var crafter = content.NewMaltHouseCrafter();
            content.FeedOneBatch(crafter);
            Tick(crafter, SixtySeconds / 2);

            crafter.SetRecipe(null);

            Assert.That(crafter.Status, Is.EqualTo(CrafterStatus.NoRecipe));
            Assert.That(crafter.Progress, Is.Zero);
            Assert.That(crafter.Input.Count(content.Barley), Is.EqualTo(10));
        }

        [Test]
        public void InputDemandBuffersTwoCrafts()
        {
            var crafter = content.NewMaltHouseCrafter();
            crafter.Input.Insert(content.Barley, 5);

            Assert.That(crafter.InputDemand(content.Barley), Is.EqualTo(15));
            Assert.That(crafter.InputDemand(content.Water), Is.EqualTo(100));
            Assert.That(crafter.InputDemand(content.PaleMalt), Is.Zero);
        }

        static void Tick(Crafter crafter, int ticks, float powerSatisfaction = 1f)
        {
            for (int i = 0; i < ticks; i++) crafter.Tick(powerSatisfaction);
        }
    }
}
