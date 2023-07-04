using System.Collections.Generic;
using UnityEngine;

public struct MazeNavigationPath
{
	/// <summary>
	/// Number of rooms in the Navigation Path.
	/// </summary>
	public int length;
	/// <summary>
	/// A list of rooms to be visited during the navigation.
	/// </summary>
	public MazeRoom[] rooms;
	/// <summary>
	/// A list of world-positions to navigate.
	/// </summary>
	public Vector3[] locations;

	/// <summary>
	/// The starting position.
	/// </summary>
	public MazePosition start;
	/// <summary>
	/// The ending position.
	/// </summary>
	public MazePosition end;
}
