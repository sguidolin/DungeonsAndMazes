using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MazeGrid
{
	private int _hash;
	private string _seed;
	private int _width, _depth;

	private float _fillRatio;
	private bool _allowOverfill;
	private MazeRoom[,] _grid;

	public string Seed => _seed;
	public int Width => _width;
	public int Depth => _depth;
	public MazeRoom[,] Grid => _grid;

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
			foreach (MazeRoom room in _grid)
				if (room.Value != 0) count++;
			return count;
		}
	}

	public bool Filled => Tiled >= Tiles;

	public MazeRoom this[int x, int y] => _grid[x, y];
	public MazeRoom this[MazePosition p] => _grid[p.x, p.y];

	public MazeGrid(string seed, int depth, int width, float fillRatio = 0.5f, bool allowOverfilling = false)
	{
		_seed = seed;
		// Calculate the hash
		_hash = ComputeSeed(seed);
		// Configure the size of the grid
		// Depth is the x-axis
		_depth = depth;
		// Width is the y-axis
		_width = width;
		// Setup the filling logic
		_fillRatio = fillRatio;
		_allowOverfill = allowOverfilling;
		// Initialize the grid
		_grid = new MazeRoom[depth, width];
		// Setup the random instance
		Random.InitState(_hash);
	}

	public IEnumerator Generate()
	{
		// Calculate the average size of the grid
		// This will be used in the random for the lifespan of the workers
		int size = (_width + _depth) / 2;
		// Instantiate a list of workers
		List<MazeWorker> workers = new List<MazeWorker>();
		// Workers all start from the same position
		MazePosition start = new MazePosition(Random.Range(0, _depth), Random.Range(0, _width));
		// Scale the number of max workers to the size by a range from 2 to 4
		int maxWorkers = size / Random.Range(2, 5);

		while (!Filled)
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
				// Dig the result of our worker's shift
				_grid.Dig(worker.Work(this));
				// After a worker has completed their shift, check if we should keep going
				// Since we might not allow overfilling then we need to stop when the quota is met
				if (!_allowOverfill && Filled) break;
			}
			// Update start position with the worker that has the most work left in them
			start = workers.OrderBy<MazeWorker, int>(worker => worker.Lifespan)
				.First<MazeWorker>().Position;
			// Finally, let those who are done retire by updating our list
			workers = workers.Where<MazeWorker>(worker => !worker.Retired).ToList<MazeWorker>();
			// Let the next batch be handled in the next frame
			yield return null;
		}
	}

	public static int ComputeSeed(string seed)
	{
		// Try converting string to int
		// If we succeed then it's our seed
		if (!int.TryParse(seed, out int hash))
		{
			// Otherwise we compute string
			using SHA1 sha = SHA1.Create();
			hash = System.BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)));
		}
		return hash;
	}
}
