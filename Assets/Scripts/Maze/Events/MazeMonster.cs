using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Maze/Events/Monster")]
public class MazeMonster : MazeEvent
{
	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		// Begin death animation
		caller.SetAnimatorTrigger("Dead");
		// Flag the actor as dead
		caller.FlagAsDead();
		// TODO: Trigger game over
		yield return null;
	}
}
