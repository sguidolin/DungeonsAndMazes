using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EventForecastPanel : MonoBehaviour
{
	private Animator _animator;

	void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void SetTrigger(string trigger)
		=> _animator.TrySetTrigger(trigger);
}
