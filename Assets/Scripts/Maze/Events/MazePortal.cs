using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Portal", menuName = "Maze/Events/Portal")]
public class MazePortal : MazeEvent
{
	[Min(0f)]
	public float travelTime = 1f;

	public override IEnumerator OnEventTrigger(ActorController caller)
	{
		// Begin teleport animation
		caller.SetAnimationTeleporting(true);
		MazeRoom target = MazeGrid.Instance.GetFreeRoom();
		// Wait for the specified travel time
		yield return new WaitForSeconds(travelTime);
		// Make the room pop in
		yield return target.RevealRoom(0f);
		// Instantly move the actor
		caller.SetPositionAndMove(target);
		// End teleport animation
		caller.SetAnimationTeleporting(false);
		yield return new WaitForEndOfFrame();
	}
}
