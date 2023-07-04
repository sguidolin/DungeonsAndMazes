using UnityEngine;

[System.Serializable]
public class MazeDoor
{
	[SerializeField]
	private Transform _location;
	[SerializeField]
	private MazeDirection _orientation;

	public Transform Location => _location;
	public MazeDirection Orientation => _orientation;
}
