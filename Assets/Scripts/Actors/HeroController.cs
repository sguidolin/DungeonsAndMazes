using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class HeroController : ActorController
{
	public int playerIndex;
	public int identifier;

	[Header("Projectiles Configuration")]
	private int _projectilesShot = 0;
	[SerializeField, Min(1)]
	private int _projectilesAvailable = 5;
	[SerializeField]
	private ProjectileController _projectile;

	[Header("UI Settings")]
	[SerializeField]
	private TextMeshPro _index;
	[SerializeField]
	private GameObject _compass;
	[SerializeField]
	private TextMeshPro _ammunitions;
	[SerializeField]
	private EventForecast _eventsForecast;

	IEnumerator Start()
	{
		Assert.IsNotNull(_ammunitions, "Ammunitions not set!");
		Assert.IsNotNull(_projectile, "Projectile not set!");
		Assert.IsNotNull(_eventsForecast, "Events Forecast not set!");
		// Disable the ammunitions
		_ammunitions.gameObject.SetActive(false);
		// Disable the compass
		_compass?.SetActive(false);
		// Disable the sprite from view
		_renderer.enabled = false;
		// Wait for the grid to be completely generated before spawning
		yield return MazeGrid.Instance.IsGenerating();
		// Enable the sprite
		_renderer.enabled = true;
		// Enable the elements to see while standing still
		_ammunitions.gameObject.SetActive(true);
		_compass?.SetActive(true);
		_isAlive = true;
		// Setup the ammunitions text
		_ammunitions.text = $"{_projectilesAvailable}/{_projectilesAvailable}";
		// Save the starting index + 1 (i.e. Player 1 would have an index of 0)
		identifier = playerIndex + 1;
		// Setup the Player indetifier on the UI
		_index.text = $"P{identifier}";
		// Disable it for just one player
		_index.gameObject.SetActive(!MazeMaster.Instance.IsMultiplayer);
		// Check for events close by
		LookForEventsInProximity();
	}

	void Update()
	{
		// Don't update if the Hero is dead
		if (_isAlive && MazeMaster.Instance.IsActiveTurn(this))
		{
			if (!_isMoving)
			{
				Vector2 moveInput = Vector2.zero;
				// Prioritize vertical movement
				// Can only move in one direction at a time
				if (Mathf.Abs(Input.GetAxis("Vertical")) > 0f)
					moveInput.x = 1f * Mathf.Sign(Input.GetAxis("Vertical"));
				else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0f)
					moveInput.y = 1f * Mathf.Sign(Input.GetAxis("Horizontal"));
				// Make sure that we can move that way
				if (moveInput != Vector2.zero && MazeGrid.Instance.IsMoveLegal(moveInput.ToDirection(), Position))
				{
					// The routine has a check built inside itself
					// But to save up we will only start it when truly needed
					StartCoroutine(Move(moveInput));
				}
				else if (_projectilesShot < _projectilesAvailable)
				{
					// If we didn't move we can calculate the shot direction
					Vector2 shotInput = Vector2.zero;
					// Always prioritize the vertical direction
					if (Mathf.Abs(Input.GetAxis("Vertical_Shot")) > 0f)
						shotInput.x = 1f * Mathf.Sign(Input.GetAxis("Vertical_Shot"));
					else if (Mathf.Abs(Input.GetAxis("Horizontal_Shot")) > 0f)
						shotInput.y = 1f * Mathf.Sign(Input.GetAxis("Horizontal_Shot"));
					// Before going into the routine we also make sure the move is legal
					if (shotInput != Vector2.zero && MazeGrid.Instance.IsMoveLegal(shotInput.ToDirection(), Position))
					{
						// This routine has a check as well
						// But we still double check before starting it
						StartCoroutine(Shoot(shotInput));
					}
				}
			}
		}
	}

	public void LookForEventsInProximity()
	{
		List<string> triggers = new List<string>();
		IEnumerable<MazeEvent> events = MazeGrid.Instance
			.GetEventsInProximity(Position);
		if (events.Any<MazeEvent>())
		{
			foreach (MazeEvent @event in MazeGrid.Instance.GetEventsInProximity(Position))
			{
				switch (@event)
				{
					case MazePortal portal:
						triggers.Add("Teleport");
						break;
					case MazeMonster monster:
						triggers.Add("Monster");
						break;
					case MazeDeathPit deathPit:
						triggers.Add("Pit");
						break;
				}
			}
		}
		_eventsForecast.SetEventForecast(triggers);
	}

	protected override IEnumerator Move(Vector2 input)
	{
		if (input != Vector2.zero)
		{
			// Reset the effects
			_eventsForecast.SetEventForecast();
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
			// Pass the turn
			MazeMaster.Instance.OnTurnCompleted();
		}
		yield return null;
	}

	IEnumerator Shoot(Vector2 input)
	{
		if (input != Vector2.zero)
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
				projectile.direction = input.ToDirection();
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
				_ammunitions.text = $"{_projectilesAvailable - _projectilesShot}/{_projectilesAvailable}";
				// Destroy the arrow instance since it's done
				Destroy(instance);
				// Only decrement ammo if we're alive, because if the actor is done it won't matter
				if (_isAlive && _projectilesAvailable - _projectilesShot <= 0)
				{
					if (MazeMaster.Instance.MonsterPositions.Any<MazePosition>())
					{
						// Trigger the player end status
						OnDeath("FadeOut");
						// Log the event
						MazeMaster.Instance.Log($"Player {identifier} ran out of ammo. They're out!");
					}
				}
			}
			// Pass the turn
			MazeMaster.Instance.OnTurnCompleted();
		}
		yield return null;
	}

	public override void OnDeath(string trigger = "")
	{
		// Apply a lock to the player
		OnLockApplied();
		// Hide the identifier
		_index.gameObject.SetActive(false);
		// Call the base method
		base.OnDeath(trigger);
	}

	public void OnVictory()
	{
		// Apply a lock to the player
		OnLockApplied();
		// Put it into cheer mode
		SetAnimatorTrigger("Cheer");
	}

	#region IBusyResource Implementation
	public override void OnLockApplied()
	{
		// Apply the lock
		IsBusy = true;

		// Hide the UI elements while locked
		_eventsForecast.gameObject.SetActive(false);
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
		if (_isAlive && !GameManager.Instance.gameOver)
		{
			// First thing is the compass
			_compass?.SetActive(true);
			// Then the ammonitions
			_ammunitions.gameObject.SetActive(true);
			// Then the forecast
			_eventsForecast.gameObject.SetActive(true);
		}

		// Release the lock
		IsBusy = false;
	}
	#endregion
}
