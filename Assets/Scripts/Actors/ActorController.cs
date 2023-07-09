using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public abstract class ActorController : MonoBehaviour, IBusyResource
{
	[SerializeField]
	protected Animator _animator;
	[SerializeField]
	protected SpriteRenderer _renderer;

	protected bool _isAlive;
	[Header("Actor Configuration")]
	[SerializeField, Min(0.5f)]
	protected float _traverseSpeed = 5f;
	[SerializeField]
	protected MazeNavigationMode _navMode = MazeNavigationMode.Locked;
	[Header("World Position")]
	[SerializeField, ReadOnly]
	protected bool _isMoving = false;
	[SerializeField, ReadOnly]
	protected MazePosition _position;

	public bool IsAlive => _isAlive;
	public bool IsMoving => _isMoving;
	public float Speed => _traverseSpeed;
	public MazePosition Position => _position;

	public bool IsBusy { get; protected set; } = false;

	void Awake()
	{
		Assert.IsNotNull(_animator, "Animator not set!");
		Assert.IsNotNull(_renderer, "Renderer not set!");
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

	public void SetAnimatorTrigger(string name)
		=> _animator.TrySetTrigger(name);
	public void SetAnimatorFlag(string name, bool value)
		=> _animator.TrySetBool(name, value);

	public virtual void OnDeath(string trigger = "")
	{
		_isAlive = false;
		// We can specify a trigger to show the animation
		if (!string.IsNullOrEmpty(trigger))
			SetAnimatorTrigger(trigger);
	}

	public void SetVisible(bool value)
		=> _renderer.enabled = value;

	protected abstract IEnumerator Move(Vector2 input);

	#region IBusyResource Implementation
	public abstract void OnLockApplied();
	public abstract void OnLockReleased();
	#endregion
}
