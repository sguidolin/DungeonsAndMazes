using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
	[Header("UI Settings")]
	public GameObject options;

	public TMP_InputField seedInput;

	public Slider depthSlider;
	public Slider widthSlider;
	public Slider ratioSlider;
	public Toggle allowTunnels;
	public TextMeshProUGUI toggleTunnels;
	public Slider playerCount;

	void Awake()
	{
		Assert.IsNotNull(options);
		Assert.IsNotNull(seedInput);
		Assert.IsNotNull(depthSlider);
		Assert.IsNotNull(widthSlider);
		Assert.IsNotNull(ratioSlider);
		Assert.IsNotNull(allowTunnels);
		Assert.IsNotNull(playerCount);
	}

	void Start()
	{
		// Clean up resources
		Resources.UnloadUnusedAssets();
	}

	public void TriggerTunnels()
	{
		toggleTunnels.text = string.Join(" ", allowTunnels.isOn ? "w/" : "w/o", "tunnels");
	}

	public void StartPlaying()
	{
		GameManager.Instance.customSettings = options.activeInHierarchy;

		if (GameManager.Instance.customSettings)
		{
			GameManager.Instance.seed = string.IsNullOrWhiteSpace(seedInput.text)
				? RandomSeedGenerator.NewSeed()
				: seedInput.text;

			GameManager.Instance.depth = (int)depthSlider.value;
			GameManager.Instance.width = (int)widthSlider.value;
			GameManager.Instance.fillRatio = ratioSlider.value / 100f;
			GameManager.Instance.allowTunnels = allowTunnels.isOn;
			GameManager.Instance.playerCount = (int)playerCount.value;
		}
		else
		{
			// Reset defaults
			GameManager.Instance.seed = string.Empty;

			GameManager.Instance.depth = 20;
			GameManager.Instance.width = 20;
			GameManager.Instance.fillRatio = 1f;
			GameManager.Instance.allowTunnels = true;
			GameManager.Instance.playerCount = 1;
		}

		SceneManager.LoadScene("Loading");
	}

	public void ViewSimulation()
		=> SceneManager.LoadScene("DungeonView");

	public void Exit()
		=> Application.Quit();
}
