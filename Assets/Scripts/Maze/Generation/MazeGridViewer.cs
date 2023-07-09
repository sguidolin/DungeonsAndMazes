using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MazeGridViewer : MonoBehaviour
{
	public bool stepByStep = false;

	[Header("UI Settings")]
	public TMP_InputField seedInput;

	public Slider depthSlider;
	public Slider widthSlider;
	public Slider ratioSlider;
	public Button generateCaller;

	public TextMeshProUGUI display;

	private MazeGrid _grid;
	private bool _isGenerating = false;

	void Awake()
	{
		enabled =
			seedInput != null &&
			depthSlider != null &&
			widthSlider != null &&
			ratioSlider != null &&
			generateCaller != null &&
			display != null;
	}

	public void Generate()
	{
		if (_isGenerating) return;
		string seed = seedInput.text;
		if (string.IsNullOrEmpty(seed))
			seed = RandomSeedGenerator.NewSeed();
		StartCoroutine(BuildDungeon(seed));
	}

	IEnumerator BuildDungeon(string seed)
	{
		if (_isGenerating) yield break;

		generateCaller.interactable = false;

		int depth = (int)depthSlider.value;
		int width = (int)widthSlider.value;
		float fillRatio = ratioSlider.value / 100f;

		_isGenerating = true;
		display.text = "Generating...";

		_grid = new MazeGrid(seed, depth, width, fillRatio);

		if (_grid.Tiles > 0)
		{
			System.DateTime startTime = System.DateTime.Now;
			IEnumerator operation = _grid.Generate(stepByStep);
			while (operation.MoveNext())
			{
				// Handle printing inside method for step-by-step
				PrintDungeon(startTime, _grid);
				yield return operation.Current;
			}
			// Once we're done, do one last print
			PrintDungeon(startTime, _grid);
		}
		else
			display.text = "Nothing to see here.";
		// Finished generating
		_isGenerating = false;

		generateCaller.interactable = true;
	}

	private void PrintDungeon(System.DateTime started, MazeGrid grid)
	{
		//float elapsed = (float)((System.DateTime.Now - started).TotalSeconds);
		//byte minutes = (byte)Mathf.FloorToInt(elapsed / 60);
		//byte seconds = (byte)Mathf.FloorToInt(elapsed % 60);
		StringBuilder layout = new StringBuilder();
		layout.AppendLine($"Seed: {_grid.Seed}");
		layout.AppendLine($"Grid is {_grid.Depth} deep and {_grid.Width} wide, with {_grid.Capacity} tiles.");
		for (int x = 0; x < grid.Depth; x++)
		{
			layout.AppendLine();
			for (int y = 0; y < grid.Width; y++)
			{
				string colorFormat = "{0}";
				if (_grid.Spawn == new MazePosition(x, y))
					colorFormat = "<color=blue>{0}</color>";
				//else
				//{
				//	if (!grid.IsOnIntegrity(new MazePosition(x, y)) && _grid[x, y].Value != 0)
				//		colorFormat = "<color=red>{0}</color>";
				//}
				layout.AppendFormat(colorFormat, _grid[x, y]);
			}
		}
		//if (_grid.IsGenerated)
		//	layout.AppendLine($"\n\nGenerated in {Mathf.RoundToInt(elapsed)} seconds. Laid {_grid.Tiled} ({((float)_grid.Tiled / (float)_grid.Capacity).ToString("P2")}) tiles.");
		//else
		//	layout.AppendLine($"\n\nGenerating... {_grid.Tiled} out of {_grid.Tiles}. Elapsed: {string.Format("{0:00}:{1:00}", minutes, seconds)}");
		if (_grid.IsGenerated)
			layout.AppendLine($"\n\nDone! Laid {_grid.Tiled} ({((float)_grid.Tiled / (float)_grid.Capacity).ToString("P2")}) tiles.");
		else
			layout.AppendLine($"\n\nGenerating... {_grid.Tiled} out of {_grid.Tiles}.");
		if (display) display.text = layout.ToString();
	}

	public void BackToMenu()
		=> SceneManager.LoadScene("Menu");
}
