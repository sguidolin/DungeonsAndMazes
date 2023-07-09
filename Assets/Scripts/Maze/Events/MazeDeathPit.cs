using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Death Pit", menuName = "Maze/Events/Death Pit")]
public class MazeDeathPit : MazeEvent
{
	public override IEnumerator OnEventTrigger(HeroController caller)
	{
		// Trigger the death
		caller.OnDeath("Fall");
		// Log the death
		MazeMaster.Instance.Log($"Player {caller.identifier} discovered gravity.");
		yield return null;
	}
}
