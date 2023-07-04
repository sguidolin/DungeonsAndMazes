using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class MazeEvent : ScriptableObject
{
	public GameObject prefab;

	public abstract IEnumerator OnEventTrigger(ActorController caller);

	public static void Spawn<T>(MazeEvent[] events, int minOccurence, int maxOccurence, MazeGridLayout grid) where T : MazeEvent
	{
		if (maxOccurence < minOccurence) maxOccurence = minOccurence;
		for (int n = 0; n < Random.Range(minOccurence, maxOccurence); n++)
		{
			// Fetch any matching event definitions
			T[] matches = events.OfType<T>().ToArray<T>();
			if (matches.Any<T>())
			{
				// If we found some, get a random one
				T @event = matches[Random.Range(0, matches.Length)];
				// Prepare to fetch a free room
				MazeRoom room = null;
				do
				{
					// Get the first available room
					room = grid.GetFreeRoom();
					// Loop until the room we got can hold an event
				} while (!MazeGrid.CanBeEvent(room));
				// We ensure that the room wasn't null, otherwise it's worth to raise an exception
				if (room == null) throw new System.Exception("Room cannot be null!");
				Debug.Log($"Spawning event \"{@event.name}\" at {room.Position}");
				room.SetEvent(Instantiate(@event));
			}
		}
	}
}
