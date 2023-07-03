using UnityEngine;

[DisallowMultipleComponent]
public class CompassBehaviour : MonoBehaviour
{
	private ActorController _actor;

	[Header("Headings")]
	[SerializeField] private GameObject _north;
	[SerializeField] private GameObject _south;
	[SerializeField] private GameObject _west;
	[SerializeField] private GameObject _east;

	void Awake()
	{
		_actor = GetComponentInParent<ActorController>();
		if (_actor == null) enabled = false;
	}

	void OnEnable()
	{
		// Set all headings as disabled
		_north.SetActive(false);
		_south.SetActive(false);
		_west.SetActive(false);
		_east.SetActive(false);
		// Only enable the active headings
		foreach (MazeDirection heading in _actor.GetLegalMoves())
		{
			switch (heading)
			{
				case MazeDirection.North:
					_north.SetActive(true);
					break;
				case MazeDirection.South:
					_south.SetActive(true);
					break;
				case MazeDirection.West:
					_west.SetActive(true);
					break;
				case MazeDirection.East:
					_east.SetActive(true);
					break;
			}
		}
	}
}
