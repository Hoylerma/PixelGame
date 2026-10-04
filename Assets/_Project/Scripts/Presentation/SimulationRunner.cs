using KettleCog.Content;
using KettleCog.Simulation;
using Unity.Profiling;
using UnityEngine;

namespace KettleCog.Presentation
{
    /// <summary>
    /// Owns the <see cref="SimulationWorld"/> and steps it at a fixed 20 ticks per second.
    /// Views read <see cref="World"/> and interpolate with <see cref="InterpolationAlpha"/>; no game state lives here.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class SimulationRunner : MonoBehaviour
    {
        static readonly ProfilerMarker StepMarker = new ProfilerMarker("KettleCog.Simulation.Step");

        [SerializeField] ContentDatabase content;
        [SerializeField, Min(0)] int startingCoins = 500;

        public SimulationWorld World { get; private set; }
        public SimulationClock Clock { get; } = new SimulationClock();

        /// <summary>0..1 progress from the last tick towards the next, for smooth belt items and animations.</summary>
        public float InterpolationAlpha => Clock.Alpha;

        void Awake()
        {
            if (content == null)
            {
                Debug.LogError($"{nameof(SimulationRunner)} needs a {nameof(ContentDatabase)} asset.", this);
                enabled = false;
                return;
            }

            World = new SimulationWorld(content.BuildRegistry(), startingCoins);
        }

        void Update()
        {
            int ticks = Clock.Advance(Time.unscaledDeltaTime);
            for (int i = 0; i < ticks; i++)
            {
                using (StepMarker.Auto()) World.Step();
            }
        }
    }
}
