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

	[SerializeField, Range(5, 30)]
	private int _depth = 20;
	[SerializeField, Range(5, 30)]
	private int _width = 20;
	[SerializeField, Range(0.25f, 1f)]
	private float _fillRatio = 0.5f;
	[SerializeField]
	private bool _allowOverfilling = false;
	[SerializeField, Range(0f, 1f)]
	private float _tunnelChance = 0.25f;
	[SerializeField]
	private bool _allowTunnels = true;

	[Space(10)]
	[SerializeField]
	private MazeRoom[] _blueprints;

	[Header("Events Configuration")]
	[SerializeField]
	private bool _buildEvents = true;
	[SerializeField, Min(1)]
	private int _minimumPortals = 1;
	[SerializeField, Range(0.01f, 0.2f)]
	private float _portalRatio = 0.05f;
	[SerializeField, Min(1)]
	private int _minimumDeathPits = 1;
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
	public string Status { get; private set; } = string.Empty;

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
		Status = "Done!";
	}

	private IEnumerator BuildGrid()
	{
		Status = "Carving the maze...";
		IEnumerator builder = _grid.Generate();
		while (builder.MoveNext())
		{
			// Set here the information for the load screen
			//Debug.Log($"Generating... {_grid.Tiled} out of {_grid.Tiles}");
			yield return builder.Current;
		}
	}

	private IEnumerator BuildLayout()
	{
		Status = "Placing the tiles...";
		for (int x = 0; x < _grid.Depth; x++)
		{
			for (int y = 0; y < _grid.Width; y++)
			{
				//Debug.Log($"Spawning room in ({x},{y})");
				// Ignore non-accessible tiles
				if (_grid[x, y] == MazeDirection.None) continue;
				// FirstOrDefault returns null when no match is found for a class
				MazeRoom blueprint = _blueprints.GetRandomTile(room => room.Tile == _grid[x, y]);
				// Now we need to check if we could place a tunnel here
				if (_allowTunnels && _grid.Spawn != new MazePosition(x, y))
				{
					float tunnelChance = _tunnelChance;
					// If it's a 4-way room I want the chance to be halved
					if (blueprint.Tile == MazeDirection.Compass)
						tunnelChance *= 0.5f;
					// Check if the blueprint can be a tunnel, and we hit the random chance
					if (MazeGrid.CanBeTunnel(blueprint) && tunnelChance >= Random.value)
					{
						// This time we look for tunnel
						blueprint = _blueprints.GetRandomTunnel(room => room.Tile == _grid[x, y]);
					}
				}
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
			}
		}
		yield return null;
	}

	private IEnumerator BuildEvents()
	{
		Status = "Generating events...";
		// Spawn Monster
		MazeEvent.Spawn<MazeMonster>(
			_events,
			1,
			1,
			this
		);
		// Spawning Death Pits
		MazeEvent.Spawn<MazeDeathPit>(
			_events,
			_minimumDeathPits,
			Mathf.RoundToInt(_grid.Tiles * _deathPitRatio),
			this
		);
		// Spawning Portals
		MazeEvent.Spawn<MazePortal>(
			_events,
			_minimumPortals,
			Mathf.RoundToInt(_grid.Tiles * _portalRatio),
			this
		);
		// All events are spawned now
		yield return null;
	}
	#endregion

	public IEnumerable<MazeDirection> GetLegalMoves(MazePosition currentPosition)
	{
		MazeRoom currentRoom = GetRoomAt(currentPosition);
		// Return the directions that match the room exits
		return MazeTile.Cardinals.Where<MazeDirection>(dir => (dir & currentRoom.Tile) != 0);
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

	public MazePosition GetSpawn()
		=> _grid.Spawn;
	public MazeRoom GetSpawnRoom()
		=> GetRoomAt(_grid.Spawn);

	public MazeRoom GetFreeRoom()
	{
		MazeRoom result = null;
		bool isValid = false;
		while (!isValid)
		{
			result = _rooms[Random.Range(0, _grid.Depth - 1), Random.Range(0, _grid.Width - 1)];
			// Ensure that result is not null, since we're not spawning walls
			// Rule is: room not empty (0), no events, not a tunnel
			// Those are special rooms that are not considered available
			isValid = result != null && result.Tile.Value > 0 && result.Event == null && !result.IsTunnel && result != GetSpawnRoom();
		}
		return result;
	}

	public MazeRoom FindRoomAt(Vector3 position)
	{
		foreach (MazeRoom room in _rooms)
			if (room.ContainsPosition(position))
				return room;
		return null;
	}

	public MazeRoom GetRoomAt(MazePosition position)
		=> GetRoomAt(position.x, position.y);
	public MazeRoom GetRoomAt(int x, int y)
		=> _rooms[x, y];

	public IEnumerable<MazeRoom> GetEvents<T>() where T : MazeEvent
		=> _rooms.Flatten().Where<MazeRoom>(room => room.Event != null && room.Event.GetType() == typeof(T));

	public bool HasEvent(MazePosition position)
		=> GetRoomAt(position).Event != null;
	public bool HasEvent(MazeRoom room)
		=> room.Event != null;

	public IEnumerable<MazeEvent> GetEventsInProximity(MazePosition position)
	{
		foreach (MazeDirection allowed in GetLegalMoves(position))
		{
			MazeRoom room = GetRoomAt(MazePosition.Move(position, allowed));
			if (HasEvent(room)) yield return room.Event;
		}
	}

#if UNITY_EDITOR
	[ContextMenu("Reveal the entire maze")]
	public void RevealMaze()
	{
		if (IsGenerated)
		{
			// Display all rooms for debug purposes
			foreach (MazeRoom room in _rooms.Flatten().Where<MazeRoom>(r => r != null))
				StartCoroutine(room.RevealRoom(0f));
		}
	}
#endif
}
