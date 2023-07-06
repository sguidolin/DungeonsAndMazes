using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class HeroController : ActorController
{
	private int _projectilesShot = 0;
	[SerializeField, Min(1)]
	private int _projectilesAvailable = 5;

	[SerializeField]
	private GameObject _compass;
	[SerializeField]
	private TextMeshPro _ammunitions;
	[SerializeField]
	private ProjectileController _projectile;

	IEnumerator Start()
	{
		Assert.IsNotNull(_ammunitions, "Ammunitions not set!");
		Assert.IsNotNull(_projectile, "Projectile not set!");
		// Disable the ammunitions
		_ammunitions.gameObject.SetActive(false);
		// Disable the compass
		_compass?.SetActive(false);
		// Disable the sprite from view
		_renderer.enabled = false;
		// Wait for the grid to be completely generated before spawning
		yield return MazeGrid.Instance.IsGenerating();
		// Find a fitting position to spawn
		MazeRoom spawn = MazeGrid.Instance.GetSpawnRoom();
		// Place the player in there
		transform.position = spawn.WorldPosition;
		_position = spawn.Position;
		// Make the spawn appear
		// Manually this time
		yield return spawn.RevealRoom(0f);
		// Enable the sprite
		_renderer.enabled = true;
		// Enable the elements to see while standing still
		_ammunitions.gameObject.SetActive(true);
		_compass?.SetActive(true);
		_isAlive = true;
		// Setup the ammunitions text
		_ammunitions.text = $"{_projectilesShot}/{_projectilesAvailable}";
		// Immediately check for events close by
		LookForEventsInProximity();
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
				// The routine has a check built inside itself
				// But to save up we will only start it when truly needed
				StartCoroutine(Move(input));
			}
			else if (_projectilesShot < _projectilesAvailable)
			{
				// If we didn't move we can calculate the shot direction
				Vector2 aim = Vector2.zero;
				// Always prioritize the vertical direction
				if (Mathf.Abs(Input.GetAxis("Vertical_Shot")) > 0f)
					aim.x = 1f * Mathf.Sign(Input.GetAxis("Vertical_Shot"));
				else if (Mathf.Abs(Input.GetAxis("Horizontal_Shot")) > 0f)
					aim.y = 1f * Mathf.Sign(Input.GetAxis("Horizontal_Shot"));
				// Before going into the routine we also make sure the move is legal
				if (aim != Vector2.zero && MazeGrid.Instance.IsMoveLegal(aim.ToDirection(), Position))
				{
					// This routine has a check as well
					// But we still double check before starting it
					StartCoroutine(Shoot(aim));
				}
			}
		}
	}

	private void LookForEventsInProximity()
	{
		IEnumerable<MazeEvent> events = MazeGrid.Instance
			.GetEventsInProximity(Position);
		if (events.Any<MazeEvent>())
		{
			foreach (MazeEvent @event in MazeGrid.Instance.GetEventsInProximity(Position))
			{
				// TODO: Enable some UI?
				// This should be done when the turn starts
				Debug.Log($"{Position} Event detected in proximity: {@event.GetType()}");
			}
		}
		else
		{
			Debug.Log($"{Position} No events in proximity");
		}
	}

	protected override IEnumerator Move(Vector2 input)
	{
		if (input != Vector2.zero)
		{
			// TODO: Disable UI or whatever message
			// Convert input into a cardinal direction
			MazeDirection movement = input.ToDirection();
			// Calculate our navigation path
			MazeNavigationPath path = MazeNavigation.Calculate(_position, movement);
			// Then traverse through the result
			yield return MazeNavigation.Navigate(this, path, true, _navMode);
			// Evaluate the room we now find ourselves in for events
			if (MazeGrid.Instance.HasEvent(Position))
			{
				// If there's an event then we want to trigger it
				MazeRoom eventRoom = MazeGrid.Instance.GetRoomAt(Position);
				// Wait for the event to run its trigger logic
				yield return eventRoom.Event.OnEventTrigger(this);
			}
			// TODO: Enable UI or whatever message
			LookForEventsInProximity();
		}
		yield return null;
	}

	IEnumerator Shoot(Vector2 aim)
	{
		if (aim != Vector2.zero)
		{
			using (this.Busy())
			{
				_projectilesShot++;
				// Instantiate the arrow prefab
				GameObject instance = Instantiate(_projectile.gameObject, MazeGrid.Instance.transform);
				// Get the controller class from the instance
				ProjectileController projectile = instance.GetComponent<ProjectileController>();
				// Set the starting values
				projectile.SetPosition(Position);
				projectile.direction = aim.ToDirection();
				// Force override the view of this element
				_ammunitions.gameObject.SetActive(true);
				_ammunitions.text = string.Empty;
				int numberOfDots = 3;
				float currentTimer = 0f;
				float secondsPerDot = 1f / numberOfDots;
				// Wait until the projectile is done traveling
				while (projectile.IsTraveling)
				{
					currentTimer += Time.deltaTime;
					if (currentTimer >= secondsPerDot)
					{
						currentTimer = 0f;
						if (_ammunitions.text.Length >= 3)
							_ammunitions.text = string.Empty;
						_ammunitions.text += ".";
					}
					yield return null;
				}
				// Update the ammunitions text
				_ammunitions.text = $"{_projectilesShot}/{_projectilesAvailable}";
				// Destroy the arrow instance since it's done
				Destroy(instance);
			}
		}
		yield return null;
	}

	#region IBusyResource Implementation
	public override void OnLockApplied()
	{
		// Apply the lock
		IsBusy = true;

		// Hide the UI elements while locked
		_ammunitions.gameObject.SetActive(false);
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
			// Then the ammonitions
			_ammunitions.gameObject.SetActive(true);
		}

		// Release the lock
		IsBusy = false;
	}
	#endregion
}
