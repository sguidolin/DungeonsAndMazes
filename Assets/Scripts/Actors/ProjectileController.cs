using System.Collections;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class ProjectileController : ActorController
{
	private BoxCollider _collider;

	public MazeDirection direction;

	public bool IsTraveling { get; private set; } = true;

	void Start()
	{
		_collider = GetComponent<BoxCollider>();

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

			direction = MazeUtilities.RotationToDirection(transform.rotation);
			// Need to figure out how to update orientation
			// If the current direction is not a legal move we set the death
			if (!MazeGrid.Instance.IsMoveLegal(direction, Position))
				_isAlive = false;
		}
		// TODO: Make some death animation
		// TODO: Evaluate whatever we need, like moving the monster
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
