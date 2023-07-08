using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Maze/Events/Monster")]
public class MazeMonster : MazeEvent
{
	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		// Trigger the death
		caller.OnDeath("Dead");
		yield return null;
	}
}
