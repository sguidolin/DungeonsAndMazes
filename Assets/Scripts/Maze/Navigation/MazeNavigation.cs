using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeNavigation
{
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

	public static IEnumerator Navigate(ActorController actor, MazeNavigationPath path)
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
			// Reveal all the rooms in the path
			foreach (MazeRoom room in path.rooms)
				yield return MazeGrid.Instance.RevealRoom(room);
			// Begin the animation
			actor.SetAnimationMoving(true);
			// Iterate through the points along the path
			for (int step = 1; step < path.locations.Length; step++)
			{
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
					// Wait for the next frame
					yield return new WaitForEndOfFrame();
				}
			}
			// Flag the actor as no longer moving
			actor.SetAnimationMoving(false);
			// Evaluate the room we find ourselves in for events
			if (MazeGrid.Instance.HasEvent(actor.Position))
			{
				// If there's an event then we want to trigger it
				MazeRoom eventRoom = MazeGrid.Instance.GetRoomAt(actor.Position);
				// Wait for the event to run its trigger logic
				yield return eventRoom.Event.OnEventTrigger(actor);
			}
			// Exiting from the scope will free up the actor automatically
		}
	}
}
