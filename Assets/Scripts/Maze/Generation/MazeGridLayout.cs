using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class MazeGridLayout : MonoBehaviour
{
	public string seed;

	[Range(1, 50)] public int depth = 20;
	[Range(1, 50)] public int width = 20;
	[Range(0f, 1f)] public float fillRatio = 0.5f;
	public bool allowOverfilling;

	public TextMeshProUGUI _display;

	private MazeGrid _grid;
	private bool _isGenerating = false;

	void Start()
	{
		if (string.IsNullOrEmpty(seed))
			NewSeed();
		else
			Generate();
	}

	void Update()
	{
		if (!_isGenerating && Input.GetKeyDown(KeyCode.R))
			NewSeed();
	}

	[ContextMenu("Generate a new dungeon")]
	public void Generate()
	{
		StartCoroutine(BuildDungeon());
	}
	[ContextMenu("Generate from a random seed")]
	public void NewSeed()
	{
		if (_isGenerating) return;
		seed = RandomSeedGenerator.NewSeed();
		StartCoroutine(BuildDungeon());
	}

	IEnumerator BuildDungeon()
	{
		if (_isGenerating) yield break;

		_isGenerating = true;
		if (_display) _display.text = "Generating...";
		_grid = new MazeGrid(seed, depth, width, fillRatio, allowOverfilling);
		float startTime = Time.time;
		IEnumerator operation = _grid.Generate();
		while (operation.MoveNext())
		{
			// Print updated status
			// ((float)_grid.Tiled / (float)_grid.Tiles).ToString("P0") <- % (no decimals)
			if (_display) _display.text = $"Generating...\n{_grid.Tiled} out of {_grid.Tiles}";
			yield return operation.Current;
		}
		StringBuilder layout = new StringBuilder();
		layout.AppendLine(_grid.Seed);
		for (int x = 0; x < depth; x++)
		{
			layout.AppendLine();
			for (int y = 0; y < width; y++)
				layout.Append(_grid[x, y]);
		}
		layout.AppendLine($"\n\nGenerated in {Mathf.RoundToInt(Time.time - startTime)} seconds.");
		if (_display) _display.text = layout.ToString();
		_isGenerating = false;
	}
}
