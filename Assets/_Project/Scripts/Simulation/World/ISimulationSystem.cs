namespace KettleCog.Simulation
{
    /// <summary>
    /// One step of the fixed tick pipeline (calendar, power, crops, grabbers, crafting, fluids, belts, ...).
    /// Systems run in the order <see cref="SimulationWorld"/> registers them; see GDD §3.3.
    /// </summary>
    public interface ISimulationSystem
    {
        void Tick(SimulationWorld world);
    }
}
