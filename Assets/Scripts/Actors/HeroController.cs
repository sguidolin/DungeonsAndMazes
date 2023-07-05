using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HeroController : ActorController
{
	[SerializeField]
	private GameObject _compass;

	IEnumerator Start()
	{
		// Disable the compass
		_compass?.SetActive(false);
		// Disable the sprite from view
		_renderer.enabled = false;
		// Wait for the grid to be completely generated before spawning
		yield return MazeGrid.Instance.IsGenerating();
		// Find a fitting position to spawn
		MazeRoom spawn = MazeGrid.Instance.GetFreeRoom();
		// Place the player in there
		transform.position = spawn.WorldPosition;
		_position = spawn.Position;
		// Make the spawn appear
		// Manually this time
		yield return spawn.RevealRoom(0f);
		// Enable the sprite
		_renderer.enabled = true;
		// Enable the compass for moving
		_compass?.SetActive(true);
		_isAlive = true;
	}

	void Update()
	{
		/*
		 * TODO: Handle Update from a turn manager
		 * So when an Actor is dead you just remove it from the list
		 */
		// Don't update if the Hero is dead
		if (!_isAlive) return;

		if (!_isMoving)
		{
			Vector2 input = Vector2.zero;
			// Prioritize vertical movement
			// Can only move in one direction at a time
			if (Mathf.Abs(Input.GetAxis("Vertical")) > 0f)
				input.x = 1f * Mathf.Sign(Input.GetAxis("Vertical"));
			else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0f)
				input.y = 1f * Mathf.Sign(Input.GetAxis("Horizontal"));
			if (input != Vector2.zero)
			{
				// Convert input into a cardinal direction
				MazeDirection movement = input.ToDirection();
				// Calculate our navigation path
				MazeNavigationPath path = MazeNavigation.Calculate(_position, movement);
				// Then traverse through the result
				StartCoroutine(MazeNavigation.Navigate(this, path, _navMode));
			}
		}
	}

	#region IBusyResource Implementation
	public override void OnLockApplied()
	{
		// Apply the lock
		IsBusy = true;

		// Hide the compass while locked
		_compass?.SetActive(false);
		// Flag the actor as moving
		_isMoving = true;
	}

	public override void OnLockReleased()
	{
		// Free up the actor on release
		_isMoving = false;
		// Enable whatever else if the actor is still alive
		if (_isAlive)
		{
			// First thing is the compass
			_compass?.SetActive(true);
		}

		// Release the lock
		IsBusy = false;
	}
	#endregion
}
