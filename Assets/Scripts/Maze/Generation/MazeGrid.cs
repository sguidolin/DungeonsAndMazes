using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MazeGrid
{
	private int _hash;
	private string _seed;
	private int _width, _depth;

	private float _fillRatio;
	private bool _allowOverfill;

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
		// Select the spawn position
		_spawn = RandomPosition();
		// Scale the number of max workers to the size by a range from 2 to 4
		int maxWorkers = size / Random.Range(2, 5);
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
						workers.Add(new MazeWorker(Random.Range(size / 2, size + 1), deploy));
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
				// Since we might not allow overfilling then we need to stop when the quota is met
				if (!_allowOverfill && IsFilled) break;
			}
			// Finally, let those who are done retire by updating our list
			workers = workers.Where<MazeWorker>(worker => !worker.Retired).ToList<MazeWorker>();
		}
		// After we filled, we need to ensure that every point is connected to the spawn
		yield return VerifyIntegrity();
		// Update the process status
		IsGenerated = true;
	}

	private IEnumerator VerifyIntegrity()
	{
		// Grab all valid tiles to evaluate
		IEnumerable<MazePosition> tiles = _grid.ToPositions()
			.Where<MazePosition>(position => _grid.GetTileAt(position).Value != 0);
		// We need to evaluate every tile
		while (tiles.Any<MazePosition>())
		{
			// Instantiate a list of validated tiles
			List<MazePosition> validated = new List<MazePosition>();
			// Only attempt one fix per iteration
			bool changed = false;
			foreach (MazePosition position in tiles)
			{
				// If we already validated this position then skip
				if (validated.Contains(position)) continue;
				// Run the pathing algorithm to see if this position can get to the spawn
				MazeNavigationTile evaluated = MazeNavigation.EnsureNavigation(_grid, position, _spawn);
				if (evaluated == null)
				{
					// We couldn't find a path
					if (!changed)
					{
						// If we haven't already attempted a fix in this iteration do it
						MazeTile tile = _grid.GetTileAt(position);
						// Ignore empty tiles
						if (tile.Value == 0)
							continue;
						// We iterate through all its possible walls
						foreach (MazeDirection direction in tile.Walls)
						{
							// On the first legal move we can go
							if (IsMoveLegal(direction, position))
							{
								// Get the next position
								MazePosition next = MazePosition.Move(position, direction);
								// After we dig we just move onto the next tile to evaluate
								_grid.Dig(position, next, direction);
								// But we actually append the one we just got
								tiles.Append<MazePosition>(next);
								// Notify the change to invalidate the data
								changed = true;
								break;
							}
						}
					}
				}
				else
				{
					// We have a path, so we iterate backwards to remove the tiles
					MazeNavigationTile valid = evaluated;
					do
					{
						validated.Add(valid.position);
						// Update the tile to its parent
						valid = valid.parent;
						// Iterate until we find a null
					} while (valid != null);
				}
				// This is quite heavy, so we might wanna offset it
				yield return null;
			}
			// Before moving to the next iteration we need to remove the validated tiles
			tiles = tiles.Except<MazePosition>(validated);
		}
		yield return null;
	}

	private MazePosition EvaluatePathTo(MazeDirection from, MazePosition start, MazePosition end)
	{
		// If we reached the end we can return that
		if (start == end) return end;
		// Check if we can move
		if (HasLegalMoves(start))
		{
			// If we can, we need to evaluate every direction
			foreach (MazeDirection direction in _grid[start.x, start.y].Entrances)
			{
				// It's the direction we came from
				// Ignore it because it's not good
				if (from != 0 && direction.Opposite() == from)
					continue;
				// Check if we have a legal move
				if (IsMoveLegal(direction, start))
				{
					// If we do, move down the evaluation
					MazePosition nextPosition = start;
					nextPosition.Move(direction);
					// This should eventually return either start or end
					return EvaluatePathTo(direction, nextPosition, end);
				}
			}
		}
		// If we can't move anywhere, then return the start
		return start;
	}

	private bool HasFreeTiles()
		=> _grid.Flatten().Any<MazeTile>(tile => tile.Value == 0);
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
	//{
	//	// Evaluate the border cases
	//	if (direction == MazeDirection.North && currentPosition.x == 0)
	//		return false;
	//	if (direction == MazeDirection.South && currentPosition.x == _depth - 1)
	//		return false;
	//	if (direction == MazeDirection.West && currentPosition.y == 0)
	//		return false;
	//	if (direction == MazeDirection.East && currentPosition.y == _width - 1)
	//		return false;
	//	return true;
	//}
	public bool HasLegalMoves(MazePosition currentPosition)
		=> _grid[currentPosition.x, currentPosition.y].Entrances.Any<MazeDirection>(direction => IsMoveLegal(direction, currentPosition));

	public int PositionToIndex(MazePosition position)
		=> position.x * _depth + position.y;
	public MazePosition IndexToPosition(int index)
		=> new MazePosition(index / _depth, index % _depth);

	// These combinations can result in a tunnel
	private static readonly MazeTile[] TUNNEL_ROOMS = new MazeTile[]
	{
		MazeDirection.North | MazeDirection.West, MazeDirection.North | MazeDirection.East,
		MazeDirection.South | MazeDirection.West, MazeDirection.South | MazeDirection.East,
		MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.East
	};

	// A tunnel must be in the combo list, and not hold an event
	public static bool CanBeTunnel(MazeRoom room)
		=> room != null && room.Position != Instance.GetSpawn() && room.Event == null && TUNNEL_ROOMS.Contains<MazeTile>(room.Tile);
	// An event must not be a tunnel, and be an actual room (not a filled block)
	public static bool CanBeEvent(MazeRoom room)
		=> room != null && room.Position != Instance.GetSpawn() && !room.IsTunnel && room.Tile != MazeTile.Block;

	public static MazeGridLayout Instance => GameManager.Instance.grid;
}
