public static class MazeUtilities
{
	public static void Dig(this MazeRoom[,] grid, MazePosition from, MazePosition to, MazeDirection direction)
	{
		// Create opening in the current room
		grid[from.x, from.y].CreateOpening(direction);
		// Create opening in the next room, by the opposite direction
		// This way the rooms are linked for sure
		grid[to.x, to.y].CreateOpening(direction.Opposite());
	}
	public static void Dig(this MazeRoom[,] grid, MazeShift shift)
		=> grid.Dig(shift.from, shift.to, shift.heading);

	public static MazeDirection Opposite(this MazeDirection from)
	{
		MazeDirection to = 0;
		if ((MazeDirection.North & from) != 0)
			to |= MazeDirection.South;
		if ((MazeDirection.South & from) != 0)
			to |= MazeDirection.North;
		if ((MazeDirection.West & from) != 0)
			to |= MazeDirection.East;
		if ((MazeDirection.East & from) != 0)
			to |= MazeDirection.West;
		return to;
	}
}
