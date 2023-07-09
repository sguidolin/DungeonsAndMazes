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
	//private readonly Color[] _playerColors = new Color[]
	//{
	//	new Color(1f, 0.25f, 0.25f), new Color(0.25f, 0.325f, 1f), new Color(0.25f, 1f, 0.25f), new Color(1f, 0.9f, 0.25f),
	//	new Color(1f, 0.25f, 1f), new Color(0f, 1f, 1f), new Color(1f, 0.5f, 0f), new Color(0.25f, 0.25f, 0.25f),
	//};

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
	[Space(10)]
	[SerializeField]
	private EventLogger _eventLogger;

	private int _activeIndex = 0;
	private List<HeroController> _players;

	public HeroController ActivePlayer
		=> _players[_activeIndex];

	public IEnumerable<HeroController> Players => _players;
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
		Assert.IsNotNull(_eventLogger, "Logger not initialized!");
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
		for (int n = 0; n < GameManager.Instance.playerCount; n++)
		{
			// Spawn the player
			GameObject instance = Instantiate(_actor.gameObject, MazeGrid.Instance.transform);
			// Rename the instance in the scene
			instance.name = $"Hero_{_players.Count}";
			// Get the controller class from the instance
			HeroController actor = instance.GetComponent<HeroController>();
			// Assign the player index
			actor.playerIndex = _players.Count;
			// Get the spawn point for the player
			MazeRoom spawn = MazeGrid.Instance.GetSpawnAt(actor.playerIndex);
			// Place the player in there
			actor.transform.position = spawn.WorldPosition;
			actor.SetPosition(spawn.Position);
			// Make the spawn appear
			// Manually this time
			yield return spawn.RevealRoom(0f);
			// Then add it to the list
			_players.Add(actor);
		}

		// Add the first player to be the camera target
		_camera.target = _players[0].gameObject;
		// Set the world position text to be the spawn room
		SetWorldStatus(_players[0].Position);

		if (_worldSeed) _worldSeed.text = MazeGrid.Instance.Seed;
		_eventLogger.Log("Player 1 turn begins!");
	}

	void OnDestroy()
	{
		// Invalidate the game manager instance
		GameManager.Instance.master = null;
	}

	public void OnTurnCompleted()
	{
		// Ignore the call if we ended the game
		if (GameManager.Instance.gameOver) return;

		// If we have an overlap, the player that arrived in the new position will kill the other
		if (_players.Any<HeroController>(player =>
			player.Position == ActivePlayer.Position && player.identifier != ActivePlayer.identifier))
		{
			// Get a list of the overlapping players
			List<HeroController> overlappingPlayers = _players
				.Where<HeroController>(player => player.identifier != ActivePlayer.identifier)
				.Where<HeroController>(player => player.Position == ActivePlayer.Position)
				.ToList<HeroController>();
			foreach (HeroController actor in overlappingPlayers)
			{
				// Log the event
				Log($"Player {ActivePlayer.identifier} and Player {actor.identifier} met! Player {actor.identifier} was defeated...");
				// Flag as dead
				actor.OnDeath("Dead");
			}
		}

		// Evaluate turn end status for the active playes
		if (_players.Any<HeroController>(player => !player.IsAlive))
		{
			// Remove each player that got flagged as dead
			List<HeroController> deadPlayers = _players
				.Where<HeroController>(player => !player.IsAlive)
				.ToList<HeroController>();
			foreach (HeroController actor in deadPlayers)
			{
				// Track what index we removed
				int removed = _players.IndexOf(actor);
				// Disable the Game Object
				actor.gameObject.SetActive(false);
				// Then remove the player
				_players.Remove(actor);

				// Decrement if needed
				if (removed <= _activeIndex)
					_activeIndex--;
			}

			// Refresh each player index
			foreach (HeroController actor in _players)
				actor.playerIndex = _players.IndexOf(actor);
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

			Log($"Player {_players[_activeIndex].identifier} turn begins!");
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

	public void Log(string text)
		=> _eventLogger.Log(text);

	public bool IsActiveTurn(HeroController actor)
		=> actor.playerIndex == _activeIndex;

	public void BackToMenu()
		=> SceneManager.LoadScene("Menu");

	public static MazeMaster Instance => GameManager.Instance.master;
}
