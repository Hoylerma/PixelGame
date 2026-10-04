namespace KettleCog.Core
{
    /// <summary>Cardinal facing of a building, belt or port. North is +Y on the grid.</summary>
    public enum Direction : byte
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    public static class DirectionExtensions
    {
        public static Direction RotateClockwise(this Direction direction) =>
            (Direction)(((int)direction + 1) & 3);

        public static Direction RotateCounterClockwise(this Direction direction) =>
            (Direction)(((int)direction + 3) & 3);

        public static Direction Opposite(this Direction direction) =>
            (Direction)(((int)direction + 2) & 3);

        /// <summary>True for East/West, where a building's footprint width and height swap.</summary>
        public static bool IsHorizontal(this Direction direction) =>
            direction == Direction.East || direction == Direction.West;

        public static GridPos ToOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return new GridPos(0, 1);
                case Direction.East: return new GridPos(1, 0);
                case Direction.South: return new GridPos(0, -1);
                default: return new GridPos(-1, 0);
            }
        }
    }
}
