using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

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
	[SerializeField]
	private TextMeshProUGUI _gameReport;

	[SerializeField]
	private Canvas _statusUI;
	[SerializeField]
	private GameStatusController _reportUI;

	private int _activeIndex = 0;
	/*
	 * TODO: A list allows us to handle multiple players
	 * Review the logic for winning/losing a game
	 * Maybe add an event log
	 * All that would allow for local multiplayer
	 */
	private List<HeroController> _players;

	public HeroController ActivePlayer
		=> _players[_activeIndex];

	public IEnumerable<MazePosition> PlayerPositions
		=> _players.Select<HeroController, MazePosition>(hero => hero.Position);
	public IEnumerable<MazePosition> MonsterPositions
		=> MazeGrid.Instance.GetEvents<MazeMonster>().Select<MazeRoom, MazePosition>(monster => monster.Position);

	void Awake()
	{
		Assert.IsNotNull(_camera, "Camera not set!");
		Assert.IsNotNull(_actor, "Actor not initialized!");
		Assert.IsNotNull(_statusUI, "Stats UI not initialized!");
		Assert.IsNotNull(_reportUI, "Report UI not initialized!");
		// Set the game manager instance
		GameManager.Instance.master = this;
		// Reset the values
		GameManager.Instance.gameOver = false;
		Time.timeScale = 1f;
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
		if (!_players[_activeIndex].IsAlive)
		{
			// Remove the player from the queue
			_players.RemoveAt(_activeIndex);
			// Decrement the index to normalize it
			_activeIndex--;
		}

		if (_players.Any<HeroController>(player => player.IsAlive))
		{
			// Increment and check the index
			if (++_activeIndex > _players.Count - 1)
				_activeIndex = 0;
			// Snap to the next player
			_camera.target = _players[_activeIndex].gameObject;
			// Set the player position in the UI
			SetWorldStatus(_players[_activeIndex].Position);
			// The next player turn starts
			// We check for events to enable the UI warnings
			_players[_activeIndex].LookForEventsInProximity();
		}
		else
		{
			// Call the game over with a failure result
			OnGameEnded(false);
		}
	}

	public void OnGameEnded(bool succeded)
	{
		GameManager.Instance.gameOver = true;
		// Disable the game UI
		_statusUI.gameObject.SetActive(false);
		// Enable the status report
		_reportUI.ShowEndScreen(succeded);
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

	public void BackToMenu()
		=> SceneManager.LoadScene("Menu");

	public static MazeMaster Instance => GameManager.Instance.master;
}
