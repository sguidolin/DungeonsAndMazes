using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGrid
{
	private int _hash;
	private string _seed;
	private int _width, _depth;

	private float _fillRatio;
	private bool _allowOverfill;
	private MazeTile[,] _grid;

	public string Seed => _seed;
	public int Width => _width;
	public int Depth => _depth;
	public MazeTile[,] Grid => _grid;

	// Dungeon tiles capacity
	public float Ratio => _fillRatio;
	public int Capacity => _width * _depth;

	public int Tiles
	{
		get
		{
			return (int)(_width * _depth * _fillRatio);
		}
	}
	public int Tiled
	{
		get
		{
			int count = 0;
			foreach (MazeTile room in _grid)
				if (room.Value != 0) count++;
			return count;
		}
	}

	public bool IsFilled => Tiled >= Tiles;
	public bool IsGenerated { get; private set; } = false;

	public MazeTile this[int x, int y] => _grid[x, y];
	public MazeTile this[MazePosition p] => _grid[p.x, p.y];

	public MazeGrid(string seed, int depth, int width, float fillRatio = 0.5f, bool allowOverfilling = false)
	{
		_seed = seed;
		// Calculate the hash
		_hash = MazeUtilities.ComputeSeed(seed);
		// Configure the size of the grid
		// Depth is the x-axis
		_depth = depth;
		// Width is the y-axis
		_width = width;
		// Setup the filling logic
		_fillRatio = fillRatio;
		_allowOverfill = allowOverfilling;
		// Initialize the grid
		_grid = new MazeTile[depth, width];
		// Setup the random instance
		Random.InitState(_hash);
	}

	public IEnumerator Generate()
	{
		// Flag as process started
		IsGenerated = false;
		// Calculate the average size of the grid
		// This will be used in the random for the lifespan of the workers
		int size = (_width + _depth) / 2;
		// Instantiate a list of workers
		List<MazeWorker> workers = new List<MazeWorker>();
		// Workers all start from the same position
		MazePosition start = new MazePosition(Random.Range(0, _depth), Random.Range(0, _width));
		// Scale the number of max workers to the size by a range from 2 to 4
		int maxWorkers = size / Random.Range(2, 5);

		while (!IsFilled)
		{
			// If we have no workers, hire more
			if (workers.Count == 0)
			{
				// The amount we get is randomized
				int recruits = Random.Range(1, maxWorkers + 1);
				// Spawn the new batch of workers
				for (int n = 0; n < recruits; n++)
				{
					// Lifespan is decided randomly between half-size and size
					workers.Add(new MazeWorker(Random.Range(size / 2, size + 1), start));
				}
			}
			// Progressively work every shift
			foreach (MazeWorker worker in workers)
			{
				MazeShift shift = worker.Work(this);
				// If we invalidated the shift then ignore it
				if (!shift.isValid) continue;
				// Dig the result of our worker's shift
				_grid.Dig(shift);
				// After a worker has completed their shift, check if we should keep going
				// Since we might not allow overfilling then we need to stop when the quota is met
				if (!_allowOverfill && IsFilled) break;
			}
			// Update start position with the worker that has the most work left in them
			start = workers.OrderBy<MazeWorker, int>(worker => worker.Lifespan)
				.First<MazeWorker>().Position;
			// Finally, let those who are done retire by updating our list
			workers = workers.Where<MazeWorker>(worker => !worker.Retired).ToList<MazeWorker>();
			// Update the process status
			IsGenerated = IsFilled;
			// Let the next batch be handled in the next frame
			yield return null;
		}
	}

	public bool IsMoveLegal(MazeDirection direction, MazePosition currentPosition)
	{
		// Evaluate the border cases
		if (direction == MazeDirection.North && currentPosition.x == 0)
			return false;
		if (direction == MazeDirection.South && currentPosition.x == _depth - 1)
			return false;
		if (direction == MazeDirection.West && currentPosition.y == 0)
			return false;
		if (direction == MazeDirection.East && currentPosition.y == _width - 1)
			return false;
		return true;
	}

	// These combinations can result in a tunnel
	private static readonly MazeTile[] TUNNEL_ROOMS = new MazeTile[]
	{
		MazeDirection.North | MazeDirection.West, MazeDirection.North | MazeDirection.East,
		MazeDirection.South | MazeDirection.West, MazeDirection.South | MazeDirection.East,
		MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.East
	};

	// A tunnel must be in the combo list, and not hold an event
	public static bool CanBeTunnel(MazeRoom room)
		=> room.Event == null && TUNNEL_ROOMS.Contains<MazeTile>(room.Tile);
	// An event must not be a tunnel, and be an actual room (not a filled block)
	public static bool CanBeEvent(MazeRoom room)
		=> !room.IsTunnel && room.Tile != MazeTile.Block;

	public static MazeGridLayout Instance => GameManager.Instance.grid;
}
