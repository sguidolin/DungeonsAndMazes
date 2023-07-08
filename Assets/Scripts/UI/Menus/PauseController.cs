using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PauseController : MonoBehaviour
{
	[SerializeField]
	private Canvas _menu;
	[SerializeField]
	private GameObject[] _deactivate;

	void Awake()
	{
		Assert.IsNotNull(_menu, "Pause menu not set!");
	}

	void Update()
	{
		if (!GameManager.Instance.gameOver)
		{
			// Static to get it done
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				TogglePause();
			}
		}
	}

	public void TogglePause()
	{
		GameManager.Instance.paused = !GameManager.Instance.paused;
		Time.timeScale = GameManager.Instance.paused ? 0f : 1f;

		_menu.gameObject.SetActive(GameManager.Instance.paused);

		foreach (GameObject go in _deactivate)
			go.SetActive(!GameManager.Instance.paused);
	}

	public void BackFromPause()
	{
		GameManager.Instance.paused = false;
		Time.timeScale = 1f;

		SceneManager.LoadScene("Menu");
	}
}
