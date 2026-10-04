using System;
using System.Collections.Generic;
using KettleCog.Core;

namespace KettleCog.Simulation
{
    /// <summary>
    /// The whole game state and its fixed-order tick pipeline. Pure C#: no UnityEngine, no wall clock,
    /// no unordered iteration — the same commands always produce the same world (GDD §3.2–3.3).
    /// Player actions arrive as <see cref="ISimCommand"/>s and are applied at the start of the next tick.
    /// </summary>
    public sealed class SimulationWorld
    {
        readonly List<ISimulationSystem> systems = new List<ISimulationSystem>();
        readonly Queue<ISimCommand> pendingCommands = new Queue<ISimCommand>();
        readonly SortedDictionary<int, BuildingInstance> buildings = new SortedDictionary<int, BuildingInstance>();
        int nextEntityId = 1;

        public SimulationWorld(ContentRegistry content, long startingCoins)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Wallet = new Wallet(startingCoins);

            // Fixed system order. Power, crops, grabbers, fluids, belts, economy and research slot in here as they land.
            systems.Add(Calendar);
            systems.Add(Crafting);
        }

        public ContentRegistry Content { get; }
        public GameCalendar Calendar { get; } = new GameCalendar();
        public Wallet Wallet { get; }
        public GridMap Grid { get; } = new GridMap();
        public CraftingSystem Crafting { get; } = new CraftingSystem();

        public long CurrentTick { get; private set; }

        /// <summary>0..1 share of power demand met this tick. Stays 1 until the power system lands.</summary>
        public float PowerSatisfaction { get; set; } = 1f;

        /// <summary>All placed buildings in id (placement) order.</summary>
        public IReadOnlyCollection<BuildingInstance> Buildings => buildings.Values;

        public event Action<BuildingInstance> BuildingPlaced;
        public event Action<BuildingInstance> BuildingRemoved;

        /// <summary>Raised when a command could not be applied, e.g. blocked tile or not enough coins.</summary>
        public event Action<ISimCommand> CommandRejected;

        public void Enqueue(ISimCommand command) =>
            pendingCommands.Enqueue(command ?? throw new ArgumentNullException(nameof(command)));

        public void Step()
        {
            while (pendingCommands.Count > 0)
            {
                var command = pendingCommands.Dequeue();
                if (!command.Execute(this)) CommandRejected?.Invoke(command);
            }

            for (int i = 0; i < systems.Count; i++) systems[i].Tick(this);
            CurrentTick++;
        }

        public bool TryGetBuilding(int id, out BuildingInstance building) => buildings.TryGetValue(id, out building);

        public bool TryGetBuildingAt(GridPos tile, out BuildingInstance building)
        {
            building = null;
            return Grid.TryGetOccupant(tile, out int id) && buildings.TryGetValue(id, out building);
        }

        internal bool TryPlaceBuilding(BuildingDef def, GridPos origin, Direction facing, out BuildingInstance building)
        {
            building = null;
            var (width, height) = def.FootprintFor(facing);
            if (!Grid.IsAreaFree(origin, width, height)) return false;
            if (!Wallet.TrySpend(def.CostCoins)) return false;

            building = new BuildingInstance(nextEntityId++, def, origin, facing);
            Grid.Occupy(origin, width, height, building.Id);
            buildings.Add(building.Id, building);
            if (building.Crafter != null) Crafting.Add(building.Crafter);

            BuildingPlaced?.Invoke(building);
            return true;
        }

        /// <summary>Removes a building with a full refund (cozy rule, GDD §4.1).</summary>
        internal bool TryRemoveBuilding(int id)
        {
            if (!buildings.TryGetValue(id, out var building)) return false;

            Grid.Release(building.Origin, building.Width, building.Height, building.Id);
            buildings.Remove(id);
            if (building.Crafter != null) Crafting.Remove(building.Crafter);
            Wallet.Refund(building.Def.CostCoins);

            BuildingRemoved?.Invoke(building);
            return true;
        }
    }
}
