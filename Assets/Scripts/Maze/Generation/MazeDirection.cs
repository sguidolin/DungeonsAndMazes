public enum MazeDirection : byte
{
	None = 0,
	North = 1 << 0,
	South = 1 << 1,
	West = 1 << 2,
	East = 1 << 3,
	NorthSouth = North | South,
	NorthWest = North | West,
	NorthEast = North | East,
	SouthWest = South | West,
	SouthEast = South | East,
	WestEast = West | East,
	NorthWestEast = North | West | East,
	SouthWestEast = South | West | East,
	NorthSouthWest = North | South | West,
	NorthSouthEast = North | South | East,
	Compass = North | South | West | East
}
