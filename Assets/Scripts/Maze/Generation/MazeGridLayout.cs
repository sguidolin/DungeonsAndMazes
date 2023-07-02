using System.Collections;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MazeGridLayout : MonoBehaviour
{
	[Header("Dungeon Configuration")]
	[SerializeField]
	private string _seed;

	[SerializeField, Range(1, 50)]
	private int _depth = 20;
	[SerializeField, Range(1, 50)]
	private int _width = 20;
	[SerializeField, Range(0f, 1f)]
	private float _fillRatio = 0.5f;
	[SerializeField]
	private bool _allowOverfilling = false;

	[Space(10)]
	[SerializeField]
	private MazeRoom[] _blueprints;

	[Header("Visual Effects")]
	[SerializeField, Range(0f, 5f)]
	private float _roomRevealTimer = 1f;

	private MazeGrid _grid;
	private MazeRoom[,] _rooms;

	//public int Depth => _grid.Depth;
	//public int Width => _grid.Width;
	//public MazeRoom[,] Rooms => _rooms;
	public bool IsGenerated { get; private set; } = false;

	//// Temporary
	//public IsometricFollow cameraTarget;

	IEnumerator Start()
	{
		// Create instance of the grid
		_grid = new MazeGrid(_seed, _depth, _width, _fillRatio, _allowOverfilling);
		// Instantiate matrix for rooms
		_rooms = new MazeRoom[_depth, _width];
		// Generate the maze
		yield return Generate();
	}

	#region Procedural Generation
	private IEnumerator Generate()
	{
		// Generate the grid first
		yield return BuildGrid();
		// Generate the layout
		yield return BuildLayout();

		//// Temporary display all rooms
		//for (int x = 0; x < _grid.Depth; x++)
		//{
		//	for (int y = 0; y < _grid.Width; y++)
		//	{
		//		Debug.Log($"Revealing room ({x}, {y})");
		//		MazeRoom room = _rooms[x, y];
		//		yield return room.RevealRoom(_roomRevealTimer);
		//	}
		//}
		IsGenerated = true;
	}

	private IEnumerator BuildGrid()
	{
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
		for (int x = 0; x < _grid.Depth; x++)
		{
			for (int y = 0; y < _grid.Width; y++)
			{
				//Debug.Log($"Spawning room in ({x},{y})");
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
		yield break;
	}
	#endregion

	public bool IsMoveLegal(MazeDirection direction, MazePosition currentPosition)
	{
		MazeRoom currentRoom = GetRoomAt(currentPosition);
		// We need to make sure that the current room has an opening in the direction
		return _grid.IsMoveLegal(direction, currentPosition) && (currentRoom.Tile & direction) != 0;
	}

	public IEnumerator RevealRoom(MazePosition position)
	{
		MazeRoom room = _rooms[position.x, position.y];
		yield return room.RevealRoom(_roomRevealTimer);
	}

	public MazeRoom GetFreeRoom()
	{
		MazeRoom result = null;
		bool isValid = false;
		while (!isValid)
		{
			result = _rooms[Random.Range(0, _grid.Depth - 1), Random.Range(0, _grid.Width - 1)];
			isValid = result.Tile.Value > 0 && result.Event == null;
		}
		return result;
	}

	public MazeRoom GetRoomAt(MazePosition position)
		=> GetRoomAt(position.x, position.y);
	public MazeRoom GetRoomAt(int x, int y)
		=> _rooms[x, y];
}
