using KettleCog.Simulation;
using NUnit.Framework;

namespace KettleCog.Tests
{
    public class InventoryTests
    {
        TestContent content;

        [SetUp]
        public void SetUp() => content = new TestContent();

        [Test]
        public void InsertTopsUpStacksBeforeUsingEmptySlots()
        {
            var inventory = new Inventory(3);

            Assert.That(inventory.Insert(content.Barley, 30), Is.EqualTo(30));
            Assert.That(inventory.Insert(content.Barley, 30), Is.EqualTo(30));

            Assert.That(inventory[0].Count, Is.EqualTo(50));
            Assert.That(inventory[1].Count, Is.EqualTo(10));
            Assert.That(inventory[2].IsEmpty, Is.True);
        }

        [Test]
        public void InsertReturnsOnlyWhatFits()
        {
            var inventory = new Inventory(2);

            Assert.That(inventory.Insert(content.Barley, 120), Is.EqualTo(100));
            Assert.That(inventory.SpaceFor(content.Barley), Is.Zero);
        }

        [Test]
        public void CanInsertAllAccountsForItemsCompetingForEmptySlots()
        {
            var inventory = new Inventory(2);
            inventory.Insert(content.Barley, 45);

            var fits = new[] { new ItemAmount(content.Barley, 5), new ItemAmount(content.PaleMalt, 50) };
            var tooMuch = new[] { new ItemAmount(content.Barley, 6), new ItemAmount(content.PaleMalt, 50) };

            Assert.That(inventory.CanInsertAll(fits), Is.True);
            Assert.That(inventory.CanInsertAll(tooMuch), Is.False);
        }

        [Test]
        public void RemoveClearsEmptiedSlots()
        {
            var inventory = new Inventory(2);
            inventory.Insert(content.Barley, 60);

            Assert.That(inventory.Remove(content.Barley, 100), Is.EqualTo(60));
            Assert.That(inventory.IsEmpty, Is.True);
            Assert.That(inventory.Insert(content.PaleMalt, 100), Is.EqualTo(100));
        }
    }
}
