using System.Security.Cryptography;
using System.Text;

public static class MazeUtilities
{
	public static void Dig(this MazeTile[,] grid, MazePosition from, MazePosition to, MazeDirection direction)
	{
		// Create opening in the current room
		grid[from.x, from.y].CreateOpening(direction);
		// Create opening in the next room, by the opposite direction
		// This way the rooms are linked for sure
		grid[to.x, to.y].CreateOpening(direction.Opposite());
	}
	public static void Dig(this MazeTile[,] grid, MazeShift shift)
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

	public static int ComputeSeed(string seed, int computations = 1)
	{
		// Try converting string to int
		// If we succeed then it's our seed
		if (!int.TryParse(seed, out int hash))
		{
			// Otherwise we compute string
			using (SHA256 sha = SHA256.Create())
			{
				// First computation
				byte[] computed = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
				// Extra shuffle, if we specify a number higher than the default (1)
				for (int i = 1; i < computations; i++) computed = sha.ComputeHash(computed);
				hash = System.BitConverter.ToInt32(computed);
			}
		}
		return hash;
	}
}
