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
}
