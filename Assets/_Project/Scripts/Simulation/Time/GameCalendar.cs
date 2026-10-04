using System;

namespace KettleCog.Simulation
{
    public enum Season : byte
    {
        Spring,
        Summer,
        Autumn,
        Winter,
    }

    /// <summary>
    /// In-game time derived purely from elapsed ticks: a 12-minute day, 7-day seasons, 4-season years (GDD §2.10).
    /// Day 0 starts at 06:00 in spring of year 1.
    /// </summary>
    public sealed class GameCalendar : ISimulationSystem
    {
        public const int RealMinutesPerDay = 12;
        public const int TicksPerDay = SimulationClock.TicksPerSecond * 60 * RealMinutesPerDay;
        public const int DaysPerSeason = 7;
        public const int SeasonsPerYear = 4;
        public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;
        public const float DayStartHour = 6f;

        public long TotalTicks { get; private set; }

        /// <summary>Absolute day index, starting at 0.</summary>
        public int Day => (int)(TotalTicks / TicksPerDay);

        /// <summary>1-based day within the current season.</summary>
        public int DayOfSeason => Day % DaysPerSeason + 1;

        public Season Season => (Season)(Day / DaysPerSeason % SeasonsPerYear);
        public int Year => Day / DaysPerYear + 1;

        /// <summary>0..1 progress through the current day.</summary>
        public float TimeOfDay => (float)(TotalTicks % TicksPerDay) / TicksPerDay;

        /// <summary>Clock hour 0..24 for UI and lighting.</summary>
        public float Hour => (DayStartHour + TimeOfDay * 24f) % 24f;

        public event Action<int> DayStarted;
        public event Action<Season> SeasonStarted;

        public void Tick(SimulationWorld world)
        {
            TotalTicks++;
            if (TotalTicks % TicksPerDay != 0) return;

            DayStarted?.Invoke(Day);
            if (Day % DaysPerSeason == 0) SeasonStarted?.Invoke(Season);
        }
    }
}
