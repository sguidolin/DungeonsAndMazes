using System.Collections.Generic;
using UnityEngine;

public static class RandomSeedGenerator
{
	private static List<string> _adjectives = new List<string>();
	private static List<string> _animals = new List<string>();

	public static string NewSeed()
	{
		// Default value
		string seed = "(No Seed)";
		if (_adjectives.Count == 0)
		{
			// Load adjectives
			TextAsset file = Resources.Load<TextAsset>("Adjectives");
			if (file != null) _adjectives.AddRange(file.text.Split(System.Environment.NewLine));
		}
		if (_animals.Count == 0)
		{
			// Load animals
			TextAsset file = Resources.Load<TextAsset>("Animals");
			if (file != null) _animals.AddRange(file.text.Split(System.Environment.NewLine));
		}
		// Generate a seed if we have populated the data
		if (_adjectives.Count > 0 && _animals.Count > 0)
			seed = string.Concat(
				_adjectives[Random.Range(0, _adjectives.Count)],
				_adjectives[Random.Range(0, _adjectives.Count)],
				_animals[Random.Range(0, _animals.Count)]
			);
		// Return whatever is stored in seed
		return seed;
	}
}
