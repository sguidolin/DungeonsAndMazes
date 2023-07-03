using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Death Pit", menuName = "Maze/Events/Death Pit")]
public class MazeDeathPit : MazeEvent
{
	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		// Begin fall animation
		caller.SetAnimationFall();
		// Flag the actor as dead
		caller.FlagAsDead();
		// TODO: Trigger game over
		yield return new WaitForEndOfFrame();
	}
}
