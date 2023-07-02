using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeDungeon
{
	/*
	 * There's going to be 3 layers
	 * 1 = The tiles
	 * 2 = The events
	 * 3 = The fog
	 * 
	 * 1 is an array of MazeTiles (or rooms?)
	 * 2 is an array of MazeEvents
	 * 3 is an array of bools
	 * 
	 * If we create a MazeRoom and force it to have a fog we can just handle that there and limit to 2 layers
	 * 
	 * How does a MazeRoom need to be setup?
	 * - It needs a MazeTile to know what kind of room it is
	 * - It could have a FogController for the fog
	 * - Maybe it could hold the MazeEvent itself and just 
	 */
	private MazeGrid _grid;
	private MazeRoom[,] _rooms;

	private MazeRoom[] _blueprints;

	public MazeDungeon(string seed, int depth, int width, float fillRatio, bool allowOverfilling, MazeRoom[] blueprints)
	{
		// Create instance of MazeGrid
		_grid = new MazeGrid(seed, depth, width, fillRatio, allowOverfilling);
		// Instantiate matrix for rooms
		_rooms = new MazeRoom[depth, width];
		// Store the array for the blueprints
		_blueprints = blueprints;
	}

	public IEnumerator Generate()
	{
		// Generate the grid first
		yield return BuildGrid();
		// Generate the layout
		yield return BuildLayout();
	}

	private IEnumerator BuildGrid()
	{
		IEnumerator builder = _grid.Generate();
		while (builder.MoveNext())
		{
			// Set here the information for the load screen
			yield return builder.Current;
		}
	}

	private IEnumerator BuildLayout()
	{
		for (int x = 0; x < _grid.Depth; x++)
		{
			for (int y = 0; y < _grid.Width; y++)
			{
				
				yield return null;
			}
		}
	}
}
