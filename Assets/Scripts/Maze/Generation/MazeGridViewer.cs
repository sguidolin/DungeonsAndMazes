using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MazeGridViewer : MonoBehaviour
{
	public bool useSmartDigging = false;

	[Header("UI Settings")]
	public TMP_InputField _seedInput;

	public Slider _depthSlider;
	public Slider _widthSlider;
	public Slider _ratioSlider;
	public Toggle _overfillToggle;

	public TextMeshProUGUI _display;

	private MazeGrid _grid;
	private bool _isGenerating = false;

	void Awake()
	{
		enabled =
			_seedInput != null &&
			_depthSlider != null &&
			_widthSlider != null &&
			_ratioSlider != null &&
			_overfillToggle != null &&
			_display != null;
	}

	void Start() => Generate();

	public void Generate()
	{
		if (_isGenerating) return;
		string seed = _seedInput.text;
		if (string.IsNullOrEmpty(seed))
			seed = RandomSeedGenerator.NewSeed();
		StartCoroutine(BuildDungeon(seed));
	}

	IEnumerator BuildDungeon(string seed)
	{
		if (_isGenerating) yield break;

		int depth = (int)_depthSlider.value;
		int width = (int)_widthSlider.value;
		float fillRatio = _ratioSlider.value;
		bool allowOverfilling = _overfillToggle.isOn;

		_isGenerating = true;
		_display.text = "Generating...";

		_grid = new MazeGrid(seed,
			depth, width, fillRatio,
			allowOverfilling, useSmartDigging);
		if (_grid.Tiles > 0)
		{
			float startTime = Time.time;
			IEnumerator operation = _grid.Generate();
			while (operation.MoveNext())
			{
				// Handle printing inside method for step-by-step
				PrintDungeon(startTime, _grid);
				yield return operation.Current;
			}
		}
		else
			_display.text = "Nothing to see here.";
		// Finished generating
		_isGenerating = false;
	}

	private void PrintDungeon(float started, MazeGrid grid)
	{
		float elapsed = Time.time - started;
		byte minutes = (byte)Mathf.FloorToInt(elapsed / 60);
		byte seconds = (byte)Mathf.FloorToInt(elapsed % 60);
		StringBuilder layout = new StringBuilder();
		layout.AppendLine($"Seed: {_grid.Seed}");
		layout.AppendLine($"Grid is {_grid.Depth} deep and {_grid.Width} wide, with {_grid.Capacity} tiles.");
		for (int x = 0; x < grid.Depth; x++)
		{
			layout.AppendLine();
			for (int y = 0; y < grid.Width; y++)
				layout.Append(_grid[x, y]);
		}
		if (_grid.IsGenerated)
			layout.AppendLine($"\n\nGenerated in {Mathf.RoundToInt(elapsed)} seconds. Laid {_grid.Tiled} ({((float)_grid.Tiled / (float)_grid.Capacity).ToString("P2")}) tiles.");
		else
			layout.AppendLine($"\n\nGenerating... {_grid.Tiled} out of {_grid.Tiles}. Elapsed: {string.Format("{0:00}:{1:00}", minutes, seconds)}");
		if (_display) _display.text = layout.ToString();
	}
}
