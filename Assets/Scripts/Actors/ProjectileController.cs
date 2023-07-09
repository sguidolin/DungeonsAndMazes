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
			// Check if we hit a player or a monster
			if (MazeMaster.Instance.PlayerPositions.Any<MazePosition>(player => player == _position))
			{
				IEnumerable<HeroController> playersHit = MazeMaster.Instance.Players
					.Where<HeroController>(player => player.Position == _position);
				// Call the on death event for each player hit
				foreach (HeroController player in playersHit)
				{
					// Trigger death
					player.OnDeath("Dead");
					// Log the event
					MazeMaster.Instance.Log($"Player {player.identifier} was hit!");
				}
				// Force exit
				IsTraveling = false;
				yield break;
			}
			else if (MazeMaster.Instance.MonsterPositions.Any<MazePosition>(monster => monster == _position))
			{
				MazeRoom monsterRoom = MazeGrid.Instance.GetRoomAt(_position);
				// Destroy the event
				monsterRoom.SetEvent(null);
				// Log the event
				MazeMaster.Instance.Log($"Player {MazeMaster.Instance.ActivePlayer.identifier} defeated the monster!");
				// Do a little dance, we killed the monster
				MazeMaster.Instance.ActivePlayer.OnVictory();
				// Trigger the victory screen
				MazeMaster.Instance.OnGameEnded(true);
				// Force exit
				IsTraveling = false;
				yield break;
			}

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
		// Log the event
		MazeMaster.Instance.Log("Your quest target has moved.");
		// Trigger the death animation
		SetAnimatorTrigger("Dead");
		IsTraveling = false;
	}

	protected override IEnumerator Move(Vector2 input)
	{
		if (input != Vector2.zero)
		{
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
