using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Maze/Events/Monster")]
public class MazeMonster : MazeEvent
{
	public override IEnumerator OnEventTrigger(HeroController caller)
	{
		// Trigger the death
		caller.OnDeath("Dead");
		// Log the death
		MazeMaster.Instance.Log($"Player {caller.identifier} failed its quest...");
		yield return null;
	}
}
