using System;

namespace KettleCog.Core
{
    /// <summary>Integer tile coordinate on the world grid. One tile is 16×16 px and one world unit.</summary>
    [Serializable]
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public static readonly GridPos Zero = new GridPos(0, 0);

        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public GridPos Offset(int dx, int dy) => new GridPos(X + dx, Y + dy);

        public GridPos Step(Direction direction) => this + direction.ToOffset();

        public int ManhattanDistance(GridPos other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        /// <summary>Chunk coordinate containing this tile. Uses floor division so negative tiles map correctly.</summary>
        public GridPos ToChunk(int chunkSize) => new GridPos(FloorDiv(X, chunkSize), FloorDiv(Y, chunkSize));

        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);

        public bool Equals(GridPos other) => this == other;
        public override bool Equals(object obj) => obj is GridPos other && this == other;
        public override int GetHashCode() => unchecked((X * 73856093) ^ (Y * 19349663));
        public override string ToString() => $"({X}, {Y})";

        static int FloorDiv(int value, int divisor) =>
            value >= 0 ? value / divisor : (value - divisor + 1) / divisor;
    }
}
