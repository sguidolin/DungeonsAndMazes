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

	private MazeGrid _grid;
	private MazeRoom[,] _rooms;

	// Temporary
	public IsometricFollow cameraTarget;

	IEnumerator Start()
	{
		// Create instance of the grid
		_grid = new MazeGrid(_seed, _depth, _width, _fillRatio, _allowOverfilling);
		// Instantiate matrix for rooms
		_rooms = new MazeRoom[_depth, _width];
		// Generate the maze
		yield return Generate();
	}

	private IEnumerator Generate()
	{
		// Generate the grid first
		yield return BuildGrid();
		// Generate the layout
		yield return BuildLayout();

		// Temporary display all rooms
		for (int x = 0; x < _grid.Depth; x++)
		{
			for (int y = 0; y < _grid.Width; y++)
			{
				Debug.Log($"Revealing room ({x}, {y})");
				MazeRoom room = _rooms[x, y];
				// Set the view on the room we're opening
				if (cameraTarget != null)
					cameraTarget.target = room.gameObject;
				yield return room.RevealRoom(0.125f);
			}
		}
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
}
