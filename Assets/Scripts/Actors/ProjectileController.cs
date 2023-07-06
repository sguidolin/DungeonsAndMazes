using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileController : ActorController
{
	public MazeDirection direction;

	public bool IsTraveling { get; private set; } = true;

	void Start()
	{
		_isAlive = true;
		_renderer.enabled = false;
		// If we set a direction which is considered pure, then we start the movement automatically
		if (direction != MazeDirection.None && MazeTile.Cardinals.Contains<MazeDirection>(direction))
			StartCoroutine(Shoot(direction));
	}

	IEnumerator Shoot(MazeDirection direction)
	{
		// Setup the starting rotation
		transform.rotation = MazeUtilities.DirectionToRotation(direction);

		_renderer.enabled = true;
		// Continue looping until we stop
		while (_isAlive)
		{
			yield return Move(direction.ToVector2());
			// TODO: We need to evaluate if we hit the player or a monster
			// If we hit anything we should handle it and also make some other animation?
			direction = MazeUtilities.RotationToDirection(transform.rotation);
			// Need to figure out how to update orientation
			// If the current direction is not a legal move we set the death
			if (!MazeGrid.Instance.IsMoveLegal(direction, Position))
				_isAlive = false;
		}
		// Get all the monsters (even though it should only be one)
		IEnumerable<MazeRoom> nests = MazeGrid.Instance.GetEvents<MazeMonster>();
		// Swap the monster with any free room that wasn't discovered yet
		// If the entire map was somehow revealed, any free room is fine
		foreach (MazeRoom monster in nests) MazeEvent.Swap(monster, MazeGrid.Instance.GetFreeRoom(true));
		// Trigger the death animation, which will invoke OnDeath
		SetAnimatorTrigger("Dead");
		IsTraveling = false;
	}

	protected override IEnumerator Move(Vector2 input)
	{
		if (input != Vector2.zero)
		{
			//MazeRoom currentRoom = MazeGrid.WorldToRoom(transform.position);
			//Debug.Log(currentRoom.Position);
			//if (currentRoom != null)
			//	_renderer.enabled = currentRoom.IsVisible;
			// Convert input into a cardinal direction
			MazeDirection movement = input.ToDirection();
			// Calculate our navigation path
			MazeNavigationPath path = MazeNavigation.Calculate(_position, movement);
			// Then traverse through the result
			yield return MazeNavigation.Navigate(this, path, false, _navMode);
		}
	}

	#region IBusyResource Implementation
	public override void OnLockApplied()
	{
		// Apply the lock
		IsBusy = true;

		// Flag the actor as moving
		_isMoving = true;
	}

	public override void OnLockReleased()
	{
		// Free up the actor on release
		_isMoving = false;

		// Release the lock
		IsBusy = false;
	}
	#endregion
}
