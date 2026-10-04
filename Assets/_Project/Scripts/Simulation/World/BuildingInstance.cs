using KettleCog.Core;

namespace KettleCog.Simulation
{
    /// <summary>A placed building. Components (crafter, and later fluid box, power node, ...) hang off it.</summary>
    public sealed class BuildingInstance
    {
        internal BuildingInstance(int id, BuildingDef def, GridPos origin, Direction facing)
        {
            Id = id;
            Def = def;
            Origin = origin;
            Facing = facing;
            var footprint = def.FootprintFor(facing);
            Width = footprint.Width;
            Height = footprint.Height;
            Crafter = def.IsCrafter ? new Crafter(def) : null;
        }

        public int Id { get; }
        public BuildingDef Def { get; }

        /// <summary>Bottom-left tile of the footprint.</summary>
        public GridPos Origin { get; }

        public Direction Facing { get; }

        /// <summary>Footprint after rotation.</summary>
        public int Width { get; }
        public int Height { get; }

        /// <summary>Null for buildings that do not craft (storage, belts, decoration).</summary>
        public Crafter Crafter { get; }

        public bool Covers(GridPos tile) =>
            tile.X >= Origin.X && tile.X < Origin.X + Width &&
            tile.Y >= Origin.Y && tile.Y < Origin.Y + Height;

        public override string ToString() => $"{Def.Id}#{Id} at {Origin}";
    }
}
