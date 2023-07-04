using UnityEngine;

[System.Serializable]
public class MazeConnection
{
	[SerializeField]
	private Transform _pivot;

	[SerializeField]
	private MazeDoor _entrance;
	[SerializeField]
	private MazeDoor _exit;

	public Transform Pivot => _pivot;
	public bool HasPivot => _pivot != null;

	public MazeDoor Entrance => _entrance;
	public bool HasEntrance => _entrance.Location != null;

	public MazeDoor Exit => _exit;
	public bool HasExit => _exit.Location != null;

}
