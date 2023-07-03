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

	private int _animatorIDDeath;
	private int _animatorIDMoving;
	private int _animatorIDTeleporting;

	void Awake()
	{
		Assert.IsFalse(_animator == null, "Animator not set!");
		Assert.IsFalse(_renderer == null, "Renderer not set!");
		// Read the animator IDs
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
	}

	void Update()
	{
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
				MazeDirection movement = input.ToDirection();
				if (MazeGrid.Instance.IsMoveLegal(movement, _position))
				{
					MazePosition nextPosition = _position;
					nextPosition.Move(movement);

					StartCoroutine(Move(_position, nextPosition));
				}
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
		_isMoving = false;

		if (MazeGrid.Instance.HasEvent(_position))
		{
			MazeRoom eventRoom = MazeGrid.Instance.GetRoomAt(_position);
			yield return eventRoom.Event.OnEventTrigger(this);
		}

		// Enable the compass again
		_compass?.SetActive(true);
	}

	public void SetPosition(MazeRoom room)
	{
		_position = room.Position;
		transform.position = room.WorldPosition;
	}

	public IEnumerable<MazeDirection> GetLegalMoves()
		=> MazeGrid.Instance.GetLegalMoves(_position);

	public void SetAnimationDeath()
		=> _animator.SetTrigger(_animatorIDDeath);
	public void SetAnimationMoving(bool value)
		=> _animator.SetBool(_animatorIDMoving, value);
	public void SetAnimationTeleporting(bool value)
		=> _animator.SetBool(_animatorIDTeleporting, value);

	#region IBusyResource Implementation
	public void OnLockApplied()
	{
		_isMoving = true;
	}

	public void OnLockReleased()
	{
		_isMoving = false;
	}
	#endregion
}
