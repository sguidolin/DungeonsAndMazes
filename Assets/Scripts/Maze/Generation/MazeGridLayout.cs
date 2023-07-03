using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MazeGridLayout : MonoBehaviour
{
	[Header("Dungeon Configuration")]
	[SerializeField]
	private string _seed;

	[SerializeField, Range(5, 50)]
	private int _depth = 20;
	[SerializeField, Range(5, 50)]
	private int _width = 20;
	[SerializeField, Range(0.25f, 1f)]
	private float _fillRatio = 0.5f;
	[SerializeField]
	private bool _allowOverfilling = false;

	[Space(10)]
	[SerializeField]
	private MazeRoom[] _blueprints;

	[Header("Events Configuration")]
	[SerializeField]
	private bool _buildEvents = true;
	[SerializeField, Range(0.01f, 0.2f)]
	private float _portalRatio = 0.05f;
	[SerializeField, Range(0.01f, 0.2f)]
	private float _deathPitRatio = 0.025f;

	[Space(10)]
	[SerializeField]
	private MazeEvent[] _events;

	[Header("Visual Effects")]
	[SerializeField, Range(0f, 5f)]
	private float _roomRevealTimer = 1f;

	private MazeGrid _grid;
	private MazeRoom[,] _rooms;

	public bool IsGenerated { get; private set; } = false;

	void Awake()
	{
		// Set the game manager instance
		GameManager.Instance.grid = this;
	}

	IEnumerator Start()
	{
		// Generate a random seed if none is provided
		if (string.IsNullOrEmpty(_seed))
			_seed = RandomSeedGenerator.NewSeed();
		// Create instance of the grid
		_grid = new MazeGrid(_seed, _depth, _width, _fillRatio, _allowOverfilling);
		// Instantiate matrix for rooms
		_rooms = new MazeRoom[_depth, _width];
		// Generate the maze
		yield return Generate();
	}

	void OnDestroy()
	{
		// Invalidate the game manager instance
		GameManager.Instance.grid = null;
	}

	public WaitWhile IsGenerating() => new WaitWhile(() => !IsGenerated);

	#region Procedural Generation
	private IEnumerator Generate()
	{
		// Generate the grid first
		yield return BuildGrid();
		// Generate the layout
		yield return BuildLayout();
		if (_buildEvents)
		{
			// Generate the events
			yield return BuildEvents();
		}
		// Flag process as complete
		IsGenerated = true;
	}

	private IEnumerator BuildGrid()
	{
		IEnumerator builder = _grid.Generate();
		while (builder.MoveNext())
		{
			// Set here the information for the load screen
			Debug.Log($"Generating... {_grid.Tiled} out of {_grid.Tiles}");
			yield return builder.Current;
		}
	}

	private IEnumerator BuildLayout()
	{
		for (int x = 0; x < _grid.Depth; x++)
		{
			for (int y = 0; y < _grid.Width; y++)
			{
				Debug.Log($"Spawning room in ({x},{y})");
				// Ignore non-accessible tiles
				if (_grid[x, y] == MazeDirection.None) continue;
				// FirstOrDefault returns null when no match is found for a class
				MazeRoom blueprint = _blueprints
					.FirstOrDefault<MazeRoom>(room => room.Tile == _grid[x, y]);
				// If we found a match we can place it
				// Otherwise we throw an exception
				if (blueprint == null) throw new System.Exception("Couldn't find a match for the blueprint.");
				// Now we instantiate our GameObject and operate on it
				GameObject instance = Instantiate(blueprint.gameObject, transform);
				// We have our instance, so we're going to fetch the MazeRoom from it
				MazeRoom room = instance.GetComponent<MazeRoom>();
				// Set the position for the room
				room.SetPosition(x, y);
				// Store the reference to the room
				_rooms[x, y] = room;
				yield return null;
			}
		}
	}

	private IEnumerator BuildEvents()
	{
		// TODO: Place the events here
		// Calculate the number of events to have based on the grid tile
		// Monster is always 1, but we could increase it for higher difficulties?
		// Portals and pits should vary, always keep them randomized but calculate the range
		// Spawn one portal for debug
		/*
		 * Chance rounded down (1,2 = 1; 0,2 = 0) and then if less than 0 -> 1
		 */
		MazePortal portal = _events.OfType<MazePortal>().FirstOrDefault<MazePortal>();
		if (portal != null)
		{
			MazeRoom portalRoom = GetFreeRoom();
			Debug.Log($"Spawning portal at {portalRoom.Position}");
			MazePortal eventInstance = Instantiate(portal);
			portalRoom.SetEvent(eventInstance);
		}
		yield break;
	}
	#endregion

	public IEnumerable<MazeDirection> GetLegalMoves(MazePosition currentPosition)
	{
		MazeRoom currentRoom = GetRoomAt(currentPosition);
		// Explicit the four cardinal directions
		MazeDirection[] cardinalDirections = new MazeDirection[]
		{
			MazeDirection.North, MazeDirection.South, MazeDirection.West, MazeDirection.East
		};

		return cardinalDirections.Where<MazeDirection>(dir => (dir & currentRoom.Tile) != 0);
	}
	public bool IsMoveLegal(MazeDirection direction, MazePosition currentPosition)
	{
		MazeRoom currentRoom = GetRoomAt(currentPosition);
		// We need to make sure that the current room has an opening in the direction
		return _grid.IsMoveLegal(direction, currentPosition) && (currentRoom.Tile & direction) != 0;
	}

	public IEnumerator RevealRoom(MazeRoom room)
	{
		yield return room.RevealRoom(_roomRevealTimer);
	}
	public IEnumerator RevealRoom(MazePosition position)
	{
		MazeRoom room = _rooms[position.x, position.y];
		yield return RevealRoom(room);
	}

	public MazeRoom GetFreeRoom()
	{
		MazeRoom result = null;
		bool isValid = false;
		while (!isValid)
		{
			result = _rooms[Random.Range(0, _grid.Depth - 1), Random.Range(0, _grid.Width - 1)];
			// Ensure that result is not null, since we're not spawning walls
			isValid = result != null && result.Tile.Value > 0 && result.Event == null;
		}
		return result;
	}

	public MazeRoom GetRoomAt(MazePosition position)
		=> GetRoomAt(position.x, position.y);
	public MazeRoom GetRoomAt(int x, int y)
		=> _rooms[x, y];

	public bool HasEvent(MazePosition position)
		=> GetRoomAt(position).Event != null;
}
