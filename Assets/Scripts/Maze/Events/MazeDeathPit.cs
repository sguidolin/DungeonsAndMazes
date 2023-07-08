using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Death Pit", menuName = "Maze/Events/Death Pit")]
public class MazeDeathPit : MazeEvent
{
	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		// Trigger the death
		caller.OnDeath("Fall");
		yield return null;
	}
}
