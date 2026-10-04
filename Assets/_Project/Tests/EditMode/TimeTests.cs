using System.Collections.Generic;
using KettleCog.Core;
using KettleCog.Simulation;
using NUnit.Framework;

namespace KettleCog.Tests
{
    public class TimeTests
    {
        [Test]
        public void ClockConvertsRealTimeIntoWholeTicks()
        {
            var clock = new SimulationClock();

            Assert.That(clock.Advance(0.12), Is.EqualTo(2));
            Assert.That(clock.Alpha, Is.EqualTo(0.4f).Within(1e-4f));
            Assert.That(clock.Advance(0.03), Is.EqualTo(1));
        }

        [Test]
        public void ClockHonoursSpeedPauseAndCatchUpLimit()
        {
            var clock = new SimulationClock { Speed = 3f };
            Assert.That(clock.Advance(0.1), Is.EqualTo(6));

            clock.Paused = true;
            Assert.That(clock.Advance(1.0), Is.Zero);

            clock.Paused = false;
            Assert.That(clock.Advance(10.0), Is.EqualTo(SimulationClock.MaxTicksPerFrame));
            Assert.That(clock.Alpha, Is.Zero, "the backlog beyond the limit is dropped");
        }

        [Test]
        public void CalendarRollsOverDaysAndSeasons()
        {
            var world = new SimulationWorld(new ContentRegistry(), startingCoins: 0);
            var days = new List<int>();
            var seasons = new List<Season>();
            world.Calendar.DayStarted += days.Add;
            world.Calendar.SeasonStarted += seasons.Add;

            for (int i = 0; i < GameCalendar.TicksPerDay * GameCalendar.DaysPerSeason; i++) world.Step();

            Assert.That(days, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
            Assert.That(seasons, Is.EqualTo(new[] { Season.Summer }));
            Assert.That(world.Calendar.Season, Is.EqualTo(Season.Summer));
            Assert.That(world.Calendar.DayOfSeason, Is.EqualTo(1));
            Assert.That(world.Calendar.Hour, Is.EqualTo(GameCalendar.DayStartHour));
        }

        [Test]
        public void GridPosMapsNegativeTilesToTheCorrectChunk()
        {
            Assert.That(new GridPos(-1, 31).ToChunk(GridMap.ChunkSize), Is.EqualTo(new GridPos(-1, 0)));
            Assert.That(new GridPos(32, -32).ToChunk(GridMap.ChunkSize), Is.EqualTo(new GridPos(1, -1)));
            Assert.That(new GridPos(-33, 0).ToChunk(GridMap.ChunkSize), Is.EqualTo(new GridPos(-2, 0)));
        }
    }
}
