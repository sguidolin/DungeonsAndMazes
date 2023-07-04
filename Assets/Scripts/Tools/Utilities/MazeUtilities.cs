using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class MazeUtilities
{
	public static BusyScope Busy(this IBusyResource resource)
		=> new BusyScope(resource);

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

	// Just returns whatever opposite directions are in `from`
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

	public static MazeDirection ToDirection(this Vector2 v)
	{
		MazeDirection dir = 0;
		if (v.x > 0f)
			dir |= MazeDirection.North;
		if (v.x < 0f)
			dir |= MazeDirection.South;
		if (v.y > 0f)
			dir |= MazeDirection.East;
		if (v.y < 0f)
			dir |= MazeDirection.West;
		return dir;
	}

	private static MazeRoom GetRandom(this MazeRoom[] collection, bool takeTunnels, System.Func<MazeRoom, bool> predicate)
	{
		// Filter out tunnels from this function
		IEnumerable<MazeRoom> filtered = collection
			.Where<MazeRoom>(room => room.IsTunnel == takeTunnels)
			.Where<MazeRoom>(predicate);
		if (filtered.Any<MazeRoom>())
		{
			// Get a random index
			int indexOfRandom = Random.Range(0, filtered.Count<MazeRoom>());
			// In order to have that we take n+1 elements from the collection
			// Then return the last one
			return filtered
				.Take<MazeRoom>(indexOfRandom + 1)
				.LastOrDefault<MazeRoom>();
		}
		return null;
	}
	public static MazeRoom GetRandomTile(this MazeRoom[] collection, System.Func<MazeRoom, bool> predicate)
		=> collection.GetRandom(false, predicate);
	public static MazeRoom GetRandomTunnel(this MazeRoom[] collection, System.Func<MazeRoom, bool> predicate)
		=> collection.GetRandom(true, predicate);

	public static void AddUnique<T>(this List<T> list, T item)
	{
		if (!list.Contains(item)) list.Add(item);
	}

	// Useful for LINQ queries
	public static IEnumerable<MazeTile> Flatten(this MazeTile[,] grid)
	{
		foreach (MazeTile tile in grid) yield return tile;
	}
	public static IEnumerable<MazeRoom> Flatten(this MazeRoom[,] grid)
	{
		foreach (MazeRoom room in grid) yield return room;
	}

	#region Cryptography
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
	#endregion
}
