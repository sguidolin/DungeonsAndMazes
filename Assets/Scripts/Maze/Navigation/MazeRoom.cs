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
	private bool _isTunnel = false;
	[SerializeField, Tooltip("Points that make up a tunnel")]
	private MazeConnection[] _paths;

	[Header("World Data")]
	[SerializeField]
	private MazePosition _position;
	[SerializeField]
	private Vector3 _worldPosition;

	public MazeTile Tile => _tile;
	public MazeEvent Event => _event;
	public bool IsTunnel => _isTunnel;

	public MazePosition Position => _position;
	public Vector3 WorldPosition => _worldPosition;
	// Return an empty array if this isn't a tunnel
	public MazeConnection[] Connections
		=> _isTunnel ? _paths : new MazeConnection[0];

	public bool IsVisible { get; private set; } = false;

	public void SetPosition(int x, int y)
		=> SetPosition(new MazePosition(x, y));
	public void SetPosition(MazePosition gridPosition)
	{
		_position = gridPosition;
		// Stored worldPosition is our localPosition really
		// It's just an internal reference to know where the pivot is in order to move between rooms
		transform.localPosition = gridPosition.ToWorldPosition(ROOM_UNIT_SIZE);
		_worldPosition = transform.localPosition;
		// Flag the room as hidden by shrinking it
		transform.localScale = Vector3.zero;
	}

	public void SetEvent(MazeEvent @event)
	{
		_event = @event;
		// Spawn the contents of the event
		Instantiate(@event.prefab, transform);
	}

	public IEnumerator RevealRoom(float duration)
	{
		// Ignore this coroutine to be safe if visible
		if (IsVisible) yield break;
		// The scale was set to 0, so we begin from there
		float currentScale = 0f;
		// Loop until we scaled it back to 1
		while (currentScale < 1f)
		{
			// Calculate the variation per frame
			float variation = (1f / duration) * Time.deltaTime;
			// Apply it to a clamped 0..1
			currentScale = Mathf.Clamp01(currentScale + variation);
			// Then grow the localScale
			transform.localScale = VectorUtils.Uniform3(currentScale);
			// Wait for the next frame
			yield return new WaitForEndOfFrame();
		}
		IsVisible = true;
	}
}
