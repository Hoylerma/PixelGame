using System.Collections.Generic;

namespace KettleCog.Simulation
{
    /// <summary>Ticks every crafter in placement order, which keeps the simulation deterministic.</summary>
    public sealed class CraftingSystem : ISimulationSystem
    {
        readonly List<Crafter> crafters = new List<Crafter>();

        public IReadOnlyList<Crafter> Crafters => crafters;

        public void Add(Crafter crafter) => crafters.Add(crafter);
        public bool Remove(Crafter crafter) => crafters.Remove(crafter);

        public void Tick(SimulationWorld world)
        {
            float powerSatisfaction = world.PowerSatisfaction;
            for (int i = 0; i < crafters.Count; i++) crafters[i].Tick(powerSatisfaction);
        }
    }
}
