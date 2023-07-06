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

	public static Vector2 ToVector2(this MazeDirection d)
	{
		Vector2 v = Vector2.zero;
		if ((d & MazeDirection.North) != 0)
			v.x += 1f;
		if ((d & MazeDirection.South) != 0)
			v.x -= 1f;
		if ((d & MazeDirection.East) != 0)
			v.y += 1f;
		if ((d & MazeDirection.West) != 0)
			v.y -= 1f;
		return v;
	}

	public static MazeDirection ToDirection(this Vector2 v)
	{
		MazeDirection d = 0;
		if (v.x > 0f)
			d |= MazeDirection.North;
		if (v.x < 0f)
			d |= MazeDirection.South;
		if (v.y > 0f)
			d |= MazeDirection.East;
		if (v.y < 0f)
			d |= MazeDirection.West;
		return d;
	}

	public static Quaternion DirectionToRotation(MazeDirection direction) =>
		direction switch
		{
			MazeDirection.North => Quaternion.Euler(0f, 90f, 0f),
			MazeDirection.West => Quaternion.Euler(0f, 0f, 0f),
			MazeDirection.East => Quaternion.Euler(0f, 180f, 0f),
			MazeDirection.South => Quaternion.Euler(0f, 270f, 0f),
			_ => Quaternion.identity
		};
	public static MazeDirection RotationToDirection(Quaternion rotation)
	{
		float angle = Mathf.Clamp(rotation.eulerAngles.y, 0f, 360f);
		// Fix angle if needed
		if (angle >= 360f) angle -= 360f;
		switch (angle)
		{
			case float y when y >= 0f && y < 90f:
				return MazeDirection.West;
			case float y when y >= 90f && y < 180f:
				return MazeDirection.North;
			case float y when y >= 180f && y < 270f:
				return MazeDirection.East;
			case float y when y >= 270f:
				return MazeDirection.South;
			default:
				Debug.Log(angle);
				return MazeDirection.None;
		}
	}


	public static bool IsMoveLegal(this MazeTile[,] grid, MazeDirection direction, MazePosition currentPosition)
	{
		// Evaluate the border cases
		if (direction == MazeDirection.North && currentPosition.x == 0)
			return false;
		if (direction == MazeDirection.South && currentPosition.x == grid.GetLength(0) - 1)
			return false;
		if (direction == MazeDirection.West && currentPosition.y == 0)
			return false;
		if (direction == MazeDirection.East && currentPosition.y == grid.GetLength(0) - 1)
			return false;
		return true;
	}

	public static MazeTile GetTileAt(this MazeTile[,] grid, MazePosition position)
		=> grid[position.x, position.y];

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

	public static IEnumerable<MazePosition> ToPositions(this MazeTile[,] grid)
	{
		for (int x = 0; x < grid.GetLength(0); x++)
			for (int y = 0; y < grid.GetLength(1); y++)
				yield return new MazePosition(x, y);
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
