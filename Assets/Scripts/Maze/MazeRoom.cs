using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MazeRoom : MonoBehaviour
{
	private const float ROOM_UNIT_SIZE = 1f;

	[Header("Room Type")]
	[SerializeField]
	private MazeTile _tile;
	private MazeEvent _event;
	[SerializeField]
	private FogController _fog;
	[SerializeField]
	private bool _isTunnel = false;

	[Header("World Data")]
	[SerializeField]
	private MazePosition _position;
	[SerializeField]
	private Vector3 _worldPosition;
	// TODO: Still evaluating how much data I would need if I were to calculate a Bezier
	[SerializeField]
	private MazeConnection[] _paths;

	public MazeTile Tile => _tile;
	public MazeEvent Event => _event;
	public bool IsTunnel => _isTunnel;

	public void SetPosition(MazePosition gridPosition)
	{
		_position = gridPosition;
		// Stored worldPosition is our localPosition really
		// It's just an internal reference to know where the pivot is in order to move between rooms
		transform.localPosition = gridPosition.ToWorldPosition();
		_worldPosition = transform.localPosition;
	}
}
