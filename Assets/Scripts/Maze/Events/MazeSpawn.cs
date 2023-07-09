using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spawn", menuName = "Maze/Events/Spawn")]
public class MazeSpawn : MazeEvent
{
	public override IEnumerator OnEventTrigger(HeroController caller)
	{
		// Nothing to do here
		MazeMaster.Instance.Log($"Player {caller.identifier} found a spawn point. How nice!");
		yield return null;
	}
}