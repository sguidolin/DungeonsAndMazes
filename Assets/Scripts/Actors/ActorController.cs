using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class ActorController : MonoBehaviour, IBusyResource
{
	[SerializeField]
	private Animator _animator;
	[SerializeField]
	private SpriteRenderer _renderer;

	private bool _isAlive;
	[Header("Actor Configuration")]
	[SerializeField, Min(0.5f)]
	private float _traverseSpeed = 5f;
	[Header("World Position")]
	[SerializeField, ReadOnly]
	private bool _isMoving = false;
	[SerializeField, ReadOnly]
	private MazePosition _position;

	[SerializeField]
	private GameObject _compass;

	private int _animatorIDFall;
	private int _animatorIDDeath;
	private int _animatorIDMoving;
	private int _animatorIDTeleporting;

	public bool IsMoving => _isMoving;
	public float Speed => _traverseSpeed;
	public MazePosition Position => _position;

	public bool IsBusy { get; private set; } = false;

	void Awake()
	{
		Assert.IsFalse(_animator == null, "Animator not set!");
		Assert.IsFalse(_renderer == null, "Renderer not set!");
		// Read the animator IDs
		_animatorIDFall = Animator.StringToHash("Fall");
		_animatorIDDeath = Animator.StringToHash("Dead");
		_animatorIDMoving = Animator.StringToHash("IsMoving");
		_animatorIDTeleporting = Animator.StringToHash("IsTeleporting");
		// Disable the compass
		_compass?.SetActive(false);
	}

	IEnumerator Start()
	{
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
		// Don't update if actor is dead
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
				//if (MazeGrid.Instance.IsMoveLegal(movement, _position))
				//{
				//	// Calculate next position
				//	MazePosition nextPosition = _position;
				//	nextPosition.Move(movement);
				//	// Start the movement routine
				//	StartCoroutine(Move(_position, nextPosition));
				//}
				// Calculate our navigation path
				MazeNavigationPath path = MazeNavigation.Calculate(_position, movement);
				// Then traverse through the result
				StartCoroutine(MazeNavigation.Navigate(this, path));
			}
		}
	}

	IEnumerator Move(MazePosition from, MazePosition to)
	{
		if (_isMoving) yield break;
		// Hide the compass during the animation
		_compass?.SetActive(false);
		// Flag the actor as moving
		_isMoving = true;
		// Update the grid position
		_position = to;
		// Get the rooms to traverse
		MazeRoom fromRoom = MazeGrid.Instance.GetRoomAt(from);
		MazeRoom toRoom = MazeGrid.Instance.GetRoomAt(to);
		// Reveal the room we're about to enter
		yield return MazeGrid.Instance.RevealRoom(toRoom);
		// Begin the animation
		SetAnimationMoving(true);
		// Current position in curve
		float currentPosition = 0f;
		// Loop until we traverse to 1f
		while (currentPosition < 1f)
		{
			// Calculate the variation for the frame
			float variation = _traverseSpeed * Time.deltaTime;
			// Apply it to a clamped 0..1
			currentPosition = Mathf.Clamp01(currentPosition + variation);
			// Update the world position
			transform.position = MathUtilities.Bezier3(currentPosition,
				fromRoom.WorldPosition, toRoom.WorldPosition);
			// Wait for the next frame
			yield return new WaitForEndOfFrame();
		}
		// Flag the actor as no longer moving
		SetAnimationMoving(false);
		// Evaluate current room for events
		if (MazeGrid.Instance.HasEvent(_position))
		{
			// If there's an event then trigger it
			MazeRoom eventRoom = MazeGrid.Instance.GetRoomAt(_position);
			yield return eventRoom.Event.OnEventTrigger(this);
		}
		// Free up operations for the controller
		_isMoving = false;
		// Enable whatever else if the actor is still alive
		if (_isAlive)
		{
			// Enable the compass again
			_compass?.SetActive(true);
		}
	}

	public void SetPosition(MazePosition position)
		=> _position = position;
	public void SetPositionAndMove(MazeRoom room)
	{
		_position = room.Position;
		transform.position = room.WorldPosition;
	}

	public IEnumerable<MazeDirection> GetLegalMoves()
		=> MazeGrid.Instance.GetLegalMoves(_position);

	public void SetAnimationFall()
		=> _animator.SetTrigger(_animatorIDFall);
	public void SetAnimationDeath()
		=> _animator.SetTrigger(_animatorIDDeath);
	public void SetAnimationMoving(bool value)
		=> _animator.SetBool(_animatorIDMoving, value);
	public void SetAnimationTeleporting(bool value)
		=> _animator.SetBool(_animatorIDTeleporting, value);

	public void FlagAsDead()
		=> _isAlive = false;

	#region IBusyResource Implementation
	public void OnLockApplied()
	{
		// Apply the lock
		IsBusy = true;

		// Hide the compass while locked
		_compass?.SetActive(false);
		// Flag the actor as moving
		_isMoving = true;
	}

	public void OnLockReleased()
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
