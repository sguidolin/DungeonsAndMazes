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

	private int[,] _map;
	private MazeTile[,] _grid;
	private MazePosition _spawn;

	public string Seed => _seed;
	public int Width => _width;
	public int Depth => _depth;
	public MazeTile[,] Grid => _grid;
	public MazePosition Spawn => _spawn;

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

	// Calculate the average size of the grid
	// This will be used in the random for the lifespan of the workers
	public int Size => (_width + _depth) / 2;

	public bool IsFilled => Tiled >= Tiles;
	public bool IsGenerated { get; private set; } = false;

	public MazeTile this[int x, int y] => _grid[x, y];
	public MazeTile this[MazePosition p] => _grid[p.x, p.y];

	public MazeGrid(string seed, int depth, int width, float fillRatio = 0.5f)
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
		// Initialize the grid
		_grid = new MazeTile[_depth, _width];
		// Initialize an empty integrity map
		_map = MazeUtilities.Matrix<int>(_depth, _width, -1);
		// Setup the random instance
		Random.InitState(_hash);
	}

	public IEnumerator Generate(bool stepByStep = false)
	{
		// Flag as process started
		IsGenerated = false;
		// Instantiate a list of workers
		List<MazeWorker> workers = new List<MazeWorker>();
		// Select the spawn position
		_spawn = RandomPosition();
		// Scale the number of max workers to the size by a range from 2 to 4
		int maxWorkers = Size / Random.Range(2, 5);
		// Use a flag to ensure a drop in the spawn
		bool spawned = false;

		while (!IsFilled)
		{
			// If we have no workers, hire more
			if (workers.Count == 0)
			{
				if (HasFreeTiles())
				{
					// The amount we get is randomized
					int recruits = Random.Range(1, maxWorkers + 1);
					// Spawn the new batch of workers
					for (int n = 0; n < recruits; n++)
					{
						// Ensure that the spawn point has a worker
						MazePosition deploy = spawned ? RandomPosition() : _spawn;
						// Lifespan is decided randomly between half-size and size
						workers.Add(new MazeWorker(MazeWorker.RandomLifespan(Size), deploy));
						// Flag that at least one worker was dropped here
						spawned = true;
					}
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
				// As soon as the quota is met we stop
				if (IsFilled) break;
			}
			// Finally, let those who are done retire by updating our list
			workers = workers.Where<MazeWorker>(worker => !worker.Retired).ToList<MazeWorker>();
			// Delay operation to give a nicer effect if requested
			if (stepByStep) yield return null;
		}
		// After we filled, we need to ensure that every point is connected to the spawn
		yield return VerifyIntegrity(stepByStep);
		// Update the process status
		IsGenerated = true;
	}

	private IEnumerator VerifyIntegrity(bool stepByStep)
	{
		while (true)
		{
			// Generate the integrity map
			_map = MazeNavigation.GetIntegrityMap(this, _spawn);
			// Find any tile that is disconnected
			IEnumerable<MazePosition> leftovers =
				MazeNavigation.GetDisconnectedPositions(this, _map);
			if (leftovers.Any<MazePosition>())
			{
				// Get the closest position
				MazePosition closest = leftovers
					.OrderBy<MazePosition, int>(position => MazePosition.Distance(position, _spawn))
					.First<MazePosition>();
				// Calculate the navigation path
				MazeNavigationTile path = MazeNavigation.GetNavigationPath(this, closest, _spawn);
				// Get all the positions we visited
				MazePosition[] points = path.GetRecursivePath().ToArray<MazePosition>();
				for (int index = 1; index < points.Length; index++)
				{
					MazePosition pointA = points[index - 1];
					MazePosition pointB = points[index];
					// Calculate the direction
					MazeDirection direction = MazePosition.GetDirection(pointA, pointB);
					// And apply the dig along the path
					_grid.Dig(pointA, pointB, direction);

					// Calculate a chance to spawn a worker to mess up our path
					float workerChance = 1f - (1f / (points.Length - index));
					// Multiply the random value to scale it and lower the odds
					if (Random.value * 0.88f < workerChance)
					{
						// Drop a new worker in our starting point
						MazeWorker worker = new MazeWorker(
							MazeWorker.RandomLifespan(Size) / points.Length, pointA
						);
						// Let it work to retirement
						while (!worker.Retired)
						{
							MazeShift shift = worker.Work(this);
							// If we invalidated the shift then ignore it
							if (!shift.isValid) continue;
							// Dig the result of our worker's shift
							_grid.Dig(shift);

							// If the worker landed on our tile then force its retirement
							if (worker.Position == pointB) worker.ForceRetirement();
						}
					}
				}
				// We only worked from the closest point, but we don't know if the grid is valid now
				// So we do nothing else and move to the next iteration
				if (stepByStep) yield return null;
			}
			else
			{
				// We have no more points to validate, so we can stop the iteration
				yield break;
			}
		}
	}

	private bool HasFreeTiles()
		=> _grid.Flatten<MazeTile>().Any<MazeTile>(tile => tile.Value == 0);
	private MazePosition RandomPosition()
	{
		if (HasFreeTiles())
		{
			MazePosition empty = new MazePosition();
			do
			{
				empty = new MazePosition(Random.Range(0, _depth), Random.Range(0, _width));
			} while (_grid[empty.x, empty.y].Value != 0);
			return empty;
		}
		return new MazePosition(Random.Range(0, _depth), Random.Range(0, _width));
	}

	public bool IsMoveLegal(MazeDirection direction, MazePosition currentPosition)
		=> _grid.IsMoveLegal(direction, currentPosition);

	public bool HasLegalMoves(MazePosition currentPosition)
		=> _grid[currentPosition.x, currentPosition.y].Entrances.Any<MazeDirection>(direction => IsMoveLegal(direction, currentPosition));

	public int PositionToIndex(MazePosition position)
		=> position.x * _depth + position.y;
	public MazePosition IndexToPosition(int index)
		=> new MazePosition(index / _depth, index % _depth);
	public bool IsOnIntegrity(MazePosition position)
		=> !(_map[position.x, position.y] < 0);

	// These combinations can result in a tunnel
	private static readonly MazeTile[] TUNNEL_ROOMS = new MazeTile[]
	{
		MazeDirection.North | MazeDirection.West, MazeDirection.North | MazeDirection.East,
		MazeDirection.South | MazeDirection.West, MazeDirection.South | MazeDirection.East,
		MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.East
	};

	// A tunnel must be in the combo list, and not hold an event
	public static bool CanBeTunnel(MazeRoom room)
		=> room != null && room.Event == null && TUNNEL_ROOMS.Contains<MazeTile>(room.Tile);
	// An event must not be a tunnel, and be an actual room (not a filled block)
	public static bool CanBeEvent(MazeRoom room)
		=> room != null && !room.IsTunnel && room.Tile != MazeTile.Block;

	public static MazeRoom FindRoomAt(Vector3 position)
		=> Instance.FindRoomAt(position);

	public static MazeGridLayout Instance => GameManager.Instance.grid;
}
