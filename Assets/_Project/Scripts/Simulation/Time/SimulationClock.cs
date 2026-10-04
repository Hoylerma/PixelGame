using System;

namespace KettleCog.Simulation
{
    /// <summary>
    /// Converts real frame time into a whole number of fixed simulation ticks (20 per second),
    /// scaled by game speed. Rendering uses <see cref="Alpha"/> to interpolate between ticks.
    /// </summary>
    public sealed class SimulationClock
    {
        public const int TicksPerSecond = 20;

        /// <summary>Catch-up limit per frame; any larger backlog is dropped instead of spiralling.</summary>
        public const int MaxTicksPerFrame = 12;

        public const float MaxSpeed = 3f;

        // Absorbs floating-point noise so e.g. 0.05 s at 20 ticks/s reliably yields one tick.
        const double TickEpsilon = 1e-9;

        // Accumulated in tick units rather than seconds: 1/20 s has no exact binary representation.
        double pendingTicks;
        float speed = 1f;

        public bool Paused { get; set; }

        /// <summary>Game speed multiplier, clamped to 0..3 (pause, 1×, 2×, 3×).</summary>
        public float Speed
        {
            get => speed;
            set => speed = value < 0f ? 0f : value > MaxSpeed ? MaxSpeed : value;
        }

        /// <summary>0..1 fraction of the way to the next tick.</summary>
        public float Alpha => (float)pendingTicks;

        /// <summary>Returns how many ticks to simulate for this frame.</summary>
        public int Advance(double realDeltaSeconds)
        {
            if (Paused || realDeltaSeconds <= 0.0) return 0;

            pendingTicks += realDeltaSeconds * speed * TicksPerSecond;
            int ticks = (int)Math.Floor(pendingTicks + TickEpsilon);
            if (ticks > MaxTicksPerFrame)
            {
                pendingTicks = 0.0;
                return MaxTicksPerFrame;
            }
            pendingTicks = Math.Max(0.0, pendingTicks - ticks);
            return ticks;
        }
    }
}
