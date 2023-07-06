using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MazeRoom : MonoBehaviour
{
	private const float ROOM_UNIT_SIZE = 1f;

	[Header("Room Type")]
	[SerializeField]
	private MazeTile _tile;
	private MazeEvent _event;
	private GameObject _eventInstance = null;
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
	public GameObject EventObject => _eventInstance;

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
		_eventInstance = Instantiate(@event.prefab, transform);
	}
	public void SetEvent(MazeEvent @event, GameObject instance)
	{
		_event = @event;
		if (instance == null)
		{
			// If we passed a null, we destroy our current reference
			if (_eventInstance != null) Destroy(_eventInstance);
		}
		else
		{
			// Move the event instance down this object
			instance.transform.parent = this.transform;
			// Update the event instance
			_eventInstance = instance;
		}
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
			transform.localScale = VectorUtils.Uniform3(MathUtilities.EaseInOutQuart(currentScale));
			// Wait for the next frame
			yield return null;
		}
		IsVisible = true;
	}

	public bool ContainsPosition(Vector3 position)
	{
		Rect roomBounds = new Rect(
			_worldPosition.x - (ROOM_UNIT_SIZE * 0.5f),
			_worldPosition.z - (ROOM_UNIT_SIZE * 0.5f),
			ROOM_UNIT_SIZE, ROOM_UNIT_SIZE
		);
		return roomBounds.Contains(position.ToVector2());
	}
}
