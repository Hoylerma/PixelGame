using System;
using System.Collections.Generic;
using KettleCog.Core;

namespace KettleCog.Simulation
{
    /// <summary>
    /// Which entity occupies each tile. Starts as a dictionary; swap for per-chunk arrays
    /// when profiling asks for it — the API stays the same (GDD §3.4).
    /// </summary>
    public sealed class GridMap
    {
        public const int ChunkSize = 32;

        readonly Dictionary<GridPos, int> occupancy = new Dictionary<GridPos, int>();

        public int OccupiedTileCount => occupancy.Count;

        public bool IsOccupied(GridPos tile) => occupancy.ContainsKey(tile);

        public bool TryGetOccupant(GridPos tile, out int entityId) => occupancy.TryGetValue(tile, out entityId);

        public bool IsAreaFree(GridPos origin, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (occupancy.ContainsKey(origin.Offset(x, y))) return false;
                }
            }
            return true;
        }

        public void Occupy(GridPos origin, int width, int height, int entityId)
        {
            if (!IsAreaFree(origin, width, height))
                throw new InvalidOperationException($"Area at {origin} ({width}×{height}) is already occupied.");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++) occupancy.Add(origin.Offset(x, y), entityId);
            }
        }

        /// <summary>Frees the area, but only tiles that actually belong to <paramref name="entityId"/>.</summary>
        public void Release(GridPos origin, int width, int height, int entityId)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = origin.Offset(x, y);
                    if (occupancy.TryGetValue(tile, out int occupant) && occupant == entityId) occupancy.Remove(tile);
                }
            }
        }
    }
}
