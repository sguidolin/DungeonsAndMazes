using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class ActorController : MonoBehaviour
{
	private Animator _animator;
	private SpriteRenderer _renderer;

	[SerializeField, Min(0.5f)]
	private float _traverseSpeed = 5f;
	private bool _isMoving = false;
	private MazePosition _position;

	[SerializeField]
	private MazeGridLayout _layout;

	void Awake()
	{
		if (_layout == null)
		{
			enabled = false;
			return;
		}

		_animator = GetComponent<Animator>();
		_renderer = GetComponent<SpriteRenderer>();
	}

	IEnumerator Start()
	{
		_renderer.enabled = false;
		// Wait for the grid to be completely generated before spawning
		yield return new WaitWhile(() => !_layout.IsGenerated);
		// Find a fitting position to spawn
		MazeRoom spawn = _layout.GetFreeRoom();
		// Place the player in there
		transform.position = spawn.WorldPosition;
		_position = spawn.Position;
		// Make the spawn appear
		// Manually this time
		yield return spawn.RevealRoom(0f);
		// Enable the sprite
		_renderer.enabled = true;
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
				if (_layout.IsMoveLegal(movement, _position))
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
		// Flag the actor as moving
		_isMoving = true;
		// Update the grid position
		_position = to;
		// Get the rooms to traverse
		MazeRoom fromRoom = _layout.GetRoomAt(from);
		MazeRoom toRoom = _layout.GetRoomAt(to);
		// Reveal the room we're about to enter
		yield return _layout.RevealRoom(toRoom.Position);
		// Begin the animation
		_animator.SetBool("IsMoving", true);
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
		_animator.SetBool("IsMoving", false);
		_isMoving = false;
	}
}
