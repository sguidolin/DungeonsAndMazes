using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Maze/Events/Monster")]
public class MazeMonster : MazeEvent
{
	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		using (caller.Busy())
		{
			// Trigger the death
			caller.OnDeath("Fall");
			// TODO: Trigger game over
			yield return null;
		}
	}
}
