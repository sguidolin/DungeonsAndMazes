using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MazeNavigationMode : byte
{
	Locked,
	RotateTowards
}

public static class MazeNavigation
{
	#region Actor Navigation
	private const float ROTATION_HARDNESS = 0.525f;

	public static MazeNavigationPath Calculate(MazePosition startPosition, MazeDirection direction)
	{
		int length = 0;
		// Using lists since we don't know the size
		List<MazeRoom> rooms = new List<MazeRoom>();
		List<Vector3> locations = new List<Vector3>();
		// Fetch the starting room for our path
		MazeRoom startRoom = MazeGrid.Instance.GetRoomAt(startPosition);
		// Add it to the list of rooms
		rooms.Add(startRoom);
		// And then add its world position to the list of locations visited
		locations.Add(startRoom.WorldPosition);
		// Now iterate the movement
		MazePosition nextPosition = startPosition;
		while (MazeGrid.Instance.IsMoveLegal(direction, nextPosition))
		{
			length++;
			nextPosition.Move(direction);
			// Fetch the next room's information
			MazeRoom nextRoom = MazeGrid.Instance.GetRoomAt(nextPosition);
			// If the next room is a tunnel, there's a bit of logic involved now
			if (nextRoom.IsTunnel)
			{
				// This is a tunnel, that means our navigation is now going to be automated
				// We enter the tunnel from the opposite direction of our movement
				MazeDirection enterFrom = direction.Opposite();
				// Now we need to detect the connected path
				// To do so we look at the room connections and find which entrance is a match
				MazeConnection connection = nextRoom.Connections
					.FirstOrDefault<MazeConnection>(conn => conn.Entrance.Orientation == enterFrom);
				// If we couldn't find a match then we throw an exception
				if (connection == null) throw new System.Exception("Couldn't find a matching room connection.");
				// Now we store the room as visited
				rooms.Add(nextRoom);
				// Get the room's world position
				Vector3 center = nextRoom.WorldPosition;
				// Add the location by using their localPosition on the room's position
				locations.AddUnique<Vector3>(center + connection.Entrance.Location.localPosition);
				locations.AddUnique<Vector3>(center + connection.Pivot.localPosition);
				locations.AddUnique<Vector3>(center + connection.Exit.Location.localPosition);
				// With this data we will end up calculating a bezier curve through the tunnel
				// But we're not done, since we need to move after a tunnel
				// Our new direction is now the orientation of the tunnel's exit
				direction = connection.Exit.Orientation;
				// We will keep moving in that direction and find the next room (or tunnel)
			}
			else
			{
				// Since it's not a tunnel, we just add it to the path
				rooms.Add(nextRoom);
				// The world position is fine in this case
				locations.Add(nextRoom.WorldPosition);
				// And we don't need to iterate anymore
				break;
			}
		}
		// Once we're done looping we return the path
		// The end will be the last position we evaluated
		return new MazeNavigationPath
		{
			length = length,
			rooms = rooms.ToArray<MazeRoom>(),
			locations = locations.ToArray<Vector3>(),
			start = startPosition,
			end = nextPosition
		};
	}

	public static IEnumerator Navigate(ActorController actor, MazeNavigationPath path,
		bool revealWhileTraveling, MazeNavigationMode mode = MazeNavigationMode.Locked)
	{
		// Ensure that the actor is available
		if (actor == null || actor.IsBusy) yield break;
		// Double-check and ensure that the path makes sense
		if (path.length == 0) yield break;
		// Apply a lock to our actor
		using (actor.Busy())
		{
			// Update the grid position right away
			actor.SetPosition(path.end);
			if (revealWhileTraveling)
			{
				// Reveal all the rooms in the path
				foreach (MazeRoom room in path.rooms)
					yield return MazeGrid.Instance.RevealRoom(room);
			}
			// Begin the animation
			actor.SetAnimatorFlag("IsMoving", true);
			// Iterate through the points along the path
			for (int step = 1; step < path.locations.Length; step++)
			{
				Vector3 previousPosition = actor.transform.position;
				// Current position in the curve
				float currentPosition = 0f;
				// Loop until we traverse to 1f (= 100%)
				while (currentPosition < 1f)
				{
					// Calculate the variation for the frame
					float variation = (actor.Speed * Time.deltaTime);
					// Speed up by 50% if we're going down a tunnel
					if (path.length > 1) variation *= 1.5f;
					// Apply it to a clamped range between 0 and 1
					currentPosition = Mathf.Clamp01(currentPosition + variation);
					// Update the world position using a bezier curve
					actor.transform.position = MathUtilities.Bezier3(
						currentPosition, path.locations[step - 1], path.locations[step]
					);
					// If the navigation mode asks for rotation, then calculate
					if (mode == MazeNavigationMode.RotateTowards)
					{
						// Calculate the rotation for the applied movement
						Quaternion rotation = Quaternion.LookRotation(
							actor.transform.position - previousPosition, Vector3.up
						);
						// Apply the rotation with some smoothing
						actor.transform.rotation = Quaternion.Slerp(
							actor.transform.rotation, rotation, ROTATION_HARDNESS
						);
						// Update the previous position for the next calculation
						previousPosition = actor.transform.position;
					}
					// Set the visibility based on the room we're currently in
					MazeRoom currentRoom = MazeGrid.Instance.FindRoomAt(actor.transform.position);
					if (currentRoom != null) actor.SetVisible(currentRoom.IsVisible);
					// Wait for the next frame
					yield return null;
				}
			}
			// Flag the actor as no longer moving
			actor.SetAnimatorFlag("IsMoving", false);
			// Exiting from the scope will free up the actor automatically
		}
	}
	#endregion

	#region Maze Integrity Validation
	private static void GraphIntegrity(MazePosition position, List<MazePosition> graphed,
		MazePosition source, MazeGrid grid, ref int[,] map)
	{
		// Iterate through every possible direction of movement
		foreach (MazeDirection direction in MazeTile.Cardinals)
		{
			// We check to make sure the move is legal
			if (grid.IsMoveLegal(direction, position))
			{
				// Calculate the next position
				MazePosition next = position;
				MazeTile tile = grid[next];
				// If the tile is empty we discard it
				if (tile.Value == 0)
					continue;
				// If we can't move there, we ignore it
				if (!tile.Entrances.Contains(direction))
					continue;
				// Move in the direction
				next.Move(direction);
				// Check if this is in the graph
				if (graphed.Contains(next))
					continue;
				// If it's not we add it
				graphed.Add(next);
				// And we calculate the map distance to the source
				map[next.x, next.y] = MazePosition.Distance(next, source);
				// Then we call this again for the next position
				GraphIntegrity(next, graphed, source, grid, ref map);
			}
		}
	}

	public static int[,] GetIntegrityMap(MazeGrid grid, MazePosition source)
	{
		// Initialize the map to the same size of the grid
		int[,] map = MazeUtilities.Matrix<int>(grid.Depth, grid.Width, -1);
		// Keep track of the positions that we already mapped
		List<MazePosition> explored = new List<MazePosition>();
		// Populate the matrix with the distance as we explore
		GraphIntegrity(source, explored, source, grid, ref map);
		// All we do is populate the map and return, handle connections elsewhere
		return map;
	}

	public static MazeNavigationTile GetNavigationPath(MazeGrid grid, MazePosition start, MazePosition end)
	{
		// This method will generate the path from start to end, ignoring walls and such
		// We will use the data from this to dig afterwards, ensuring the connections

		// Create the starting tile for our navigation
		MazeNavigationTile startingTile = new MazeNavigationTile
		{
			position = start
		};
		// Create the ending tile for our navigation
		MazeNavigationTile endingTile = new MazeNavigationTile
		{
			position = end
		};
		// Instantiate the list of active tiles to check
		List<MazeNavigationTile> activeTiles = new List<MazeNavigationTile>();
		// The first one is the start
		activeTiles.Add(startingTile);
		// Instantiate a list to keep track of the visited tiles
		List<MazeNavigationTile> visitedTiles = new List<MazeNavigationTile>();

		while (activeTiles.Any<MazeNavigationTile>())
		{
			// Get the cheapest tile
			MazeNavigationTile checkTile = activeTiles
				.OrderBy<MazeNavigationTile, int>(tile => tile.Weight)
				.First<MazeNavigationTile>();

			if (checkTile.position == endingTile.position)
			{
				// We found the destination
				return checkTile;
			}

			// Move the tile from active to visited
			visitedTiles.Add(checkTile);
			activeTiles.Remove(checkTile);

			// Get the tiles that we can reach from here
			IEnumerable<MazeNavigationTile> walkableTiles = GetReachableTiles(grid, checkTile, endingTile);
			// Iterate through each one to check if it's an active tile
			foreach (MazeNavigationTile walkableTile in walkableTiles)
			{
				// We have already visited this tile, so we don't care about it
				if (visitedTiles.Any<MazeNavigationTile>(tile => tile.position == walkableTile.position))
					continue;
				// If it's already in the active list, but has a better cost, then we can evaluate it
				if (activeTiles.Any<MazeNavigationTile>(tile => tile.position == walkableTile.position))
				{
					MazeNavigationTile existingTile = activeTiles
						.First<MazeNavigationTile>(tile => tile.position == walkableTile.position);
					if (existingTile.Weight > checkTile.Weight)
					{
						activeTiles.Remove(existingTile);
						activeTiles.Add(walkableTile);
					}
				}
				else
				{
					// We've never been here, so add it to the list
					activeTiles.Add(walkableTile);
				}
			}
		}
		// If we couldn't find a path, we return null
		return null;
	}

	private static IEnumerable<MazeNavigationTile> GetReachableTiles(MazeGrid grid, MazeNavigationTile current, MazeNavigationTile target)
	{
		List<MazeNavigationTile> possibleTiles = new List<MazeNavigationTile>();
		// Iterate through every direction we can move
		foreach (MazeDirection direction in MazeTile.Cardinals)
		{
			// We check to make sure the move is legal
			if (grid.IsMoveLegal(direction, current.position))
			{
				MazePosition possiblePosition = current.position;
				// Move in the direction to add the new tile
				possiblePosition.Move(direction);
				// If we have a parent we already went through an iteration
				// So if we try to move towards our parent -- don't
				if (current.parent != null && current.parent.position == possiblePosition)
					continue;
				// Add the tile to the possibilities
				possibleTiles.Add(new MazeNavigationTile
				{
					position = possiblePosition,
					cost = current.cost + 1,
					parent = current,
				});
			}
		}
		// Calculate the distance for each tile
		possibleTiles.ForEach(tile => tile.SetDistance(target.position));
		// Return whatever we found
		return possibleTiles;
	}

	public static IEnumerable<MazePosition> GetDisconnectedPositions(MazeGrid grid, int[,] map)
	{
		for (int x = 0; x < grid.Depth; x++)
		{
			for (int y = 0; y < grid.Width; y++)
			{
				if (map[x, y] < 0 && grid[x, y].Value > 0)
					yield return new MazePosition(x, y);
			}
		}
	}

	public static IEnumerable<MazePosition> GetRecursivePath(this MazeNavigationTile path)
	{
		MazeNavigationTile tile = path;
		// Iterate until we find a null
		while (tile != null)
		{
			// Return the position
			yield return tile.position;
			// Update the tile to its parent
			tile = tile.parent;
		}
	}
	#endregion
}
