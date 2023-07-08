using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class MazeMaster : MonoBehaviour
{
	[SerializeField]
	private IsometricFollow _camera;
	[SerializeField]
	private HeroController _actor;

	[Header("UI Settings")]
	[SerializeField]
	private TextMeshProUGUI _worldPosition;
	[SerializeField]
	private TextMeshProUGUI _worldSeed;

	private int _activeIndex = 0;
	private List<HeroController> _players;

	public IEnumerable<MazePosition> PlayerPositions
		=> _players.Select<HeroController, MazePosition>(hero => hero.Position);
	public IEnumerable<MazePosition> MonsterPositions
		=> MazeGrid.Instance.GetEvents<MazeMonster>().Select<MazeRoom, MazePosition>(monster => monster.Position);

	void Awake()
	{
		Assert.IsNotNull(_camera, "Camera not set!");
		Assert.IsNotNull(_actor, "Actor not initialized!");
		// Set the game manager instance
		GameManager.Instance.master = this;
	}

	IEnumerator Start()
	{
		// Initialize the list of players
		_players = new List<HeroController>();
		// Wait for the grid to be completely generated before spawning
		yield return MazeGrid.Instance.IsGenerating();
		// Spawn the player
		GameObject instance = Instantiate(_actor.gameObject, MazeGrid.Instance.transform);
		// Rename the instance in the scene
		instance.name = $"Hero_{_players.Count}";
		// Get the controller class from the instance
		HeroController actor = instance.GetComponent<HeroController>();
		// Assign the player index
		actor.playerIndex = _players.Count;
		// Then add it to the list
		_players.Add(actor);

		// Add the first player to be the camera target
		_camera.target = _players[0].gameObject;
		// Set the world position text to be the spawn room
		SetWorldStatus(MazeGrid.Instance.GetSpawn());

		if (_worldSeed) _worldSeed.text = MazeGrid.Instance.Seed;
	}

	public void OnTurnCompleted()
	{
		// Evaluate turn end status for the active player
		// Increment and check the index
		if (++_activeIndex > _players.Count - 1)
			_activeIndex = 0;
		// TODO: Shift the camera?
		_camera.target = _players[_activeIndex].gameObject;
		// Set the player position in the UI
		SetWorldStatus(_players[_activeIndex].Position);
		// The next player turn starts
		// We check for events to enable the UI warnings
		_players[_activeIndex].LookForEventsInProximity();
	}

	private void SetWorldStatus(MazePosition position)
	{
		if (_worldPosition)
		{
			// Include the % of the map that has been revealed
			_worldPosition.text = $"{MathUtilities.Round(MazeGrid.Instance.Revealed, 0)}%" +
				$"{System.Environment.NewLine}" +
				$"{position}";
		}
	}

	public bool IsActiveTurn(HeroController actor)
		=> actor.playerIndex == _activeIndex;

	public static MazeMaster Instance => GameManager.Instance.master;
}
