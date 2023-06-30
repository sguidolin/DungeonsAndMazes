using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MazeGridLayout : MonoBehaviour
{
	public string seed;

	[Range(1, 50)] public int depth = 20;
	[Range(1, 50)] public int width = 20;
	[Range(0f, 1f)] public float fillRatio = 0.5f;
	public bool allowOverfilling;

	public Text _display;

	private MazeGrid _grid;
	private bool _isGenerating = false;

	void Start()
	{
		Generate();
	}

	[ContextMenu("Generate a new dungeon")]
	public void Generate()
	{
		StartCoroutine(BuildDungeon());
	}

	IEnumerator BuildDungeon()
	{
		if (_isGenerating) yield break;

		_isGenerating = true;
		if (_display) _display.text = "Generating...";
		//Debug.Log($"[{Time.time}] Starting dungeon generation...");
		_grid = new MazeGrid(seed, depth, width, fillRatio, allowOverfilling);
		IEnumerator operation = _grid.Generate();
		while (operation.MoveNext())
		{
			// Print updated status
			if (_display) _display.text = $"Generating...\n{_grid.Tiled} out of {_grid.Tiles}";
			yield return operation.Current;
		}
		//yield return _grid.Generate();
		//Debug.Log($"[{Time.time}] Dungeon generated!");
		StringBuilder layout = new StringBuilder();
		layout.AppendLine(_grid.Seed);
		for (int x = 0; x < depth; x++)
		{
			layout.AppendLine();
			for (int y = 0; y < width; y++)
				layout.Append(_grid[x, y]);
		}
		if (_display) _display.text = layout.ToString();
		_isGenerating = false;
	}
}
