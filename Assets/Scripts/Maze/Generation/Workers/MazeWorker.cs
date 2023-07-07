using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class MazeWorker
{
	private const float BIAS_FACTOR = 0.215f;
	private const float ADVENTUROUS_FACTOR = 1.55f;
	private const float SECOND_WIND_CHANCE = 0.25f;

	private MazePosition _position;
	private int _lifespan;

	// Random factors
	private float _bias;
	private float _indecisiveness;
	private float _adventurous;

	private List<MazeDirection> _choices;

	public int Lifespan => _lifespan;
	public bool Retired => !(_lifespan > 0);
	public MazePosition Position => _position;

	public MazeWorker(int lifespan, MazePosition deployment)
	{
		_lifespan = lifespan;
		_position = deployment;
		// What's the chance for bias of our worker?
		_bias = Random.value * BIAS_FACTOR;
		// Is our worker seeking to build new tunnels?
		_adventurous = (Random.value * ADVENTUROUS_FACTOR) + 0.025f;
		// Initialize the choices the worker can make
		_choices = new List<MazeDirection>(MazeTile.Cardinals);
		// Bias is the chance that our worker will dislike a random direction
		while (Random.value < _bias)
		{
			// If we had a bias proc we remove a choice from their work
			// If we only have one choice left then we can break the loop
			if (_choices.Count > 1)
				_choices.Remove(_choices[Random.Range(0, _choices.Count)]);
			else
				break;
		}
	}

	private bool HasLegalMoves(List<MazeDirection> choices, MazeGrid grid)
		=> choices.Any<MazeDirection>(direction => grid.IsMoveLegal(direction, _position));

	public MazeShift Work(MazeGrid locale)
	{
		bool isMoveValid = false;
		MazeDirection direction = 0;
		MazePosition startPosition = _position;
		// Instantiate a new list of choices
		List<MazeDirection> possibleChoices = _choices;

		while (!isMoveValid)
		{
			// Make sure the worker has legal choices to make
			if (HasLegalMoves(possibleChoices, locale))
			{
				// Let the worker pick a direction
				direction = possibleChoices[Random.Range(0, possibleChoices.Count)];
				// Check validity of the move we picked
				isMoveValid = locale.IsMoveLegal(direction, _position);
				// If we can move, we need to evaluate if we actually want to go through
				// This is based on the number of holes that have been dug already in the next cell
				if (isMoveValid)
				{
					// Get the next position
					MazePosition nextPosition = _position;
					nextPosition.Move(direction);
					// Get the next room
					MazeTile nextRoom = locale[nextPosition];
					// Count the holes that have been dug
					int holes = nextRoom.Openings;
					// If it's 0 or all we go through
					if (holes == 0 || holes == MazeTile.Cardinals.Length) continue;
					// Calculate the chance to advance
					float advanceChance = 1f / (holes * _adventurous);
					// If we meet the chance then we continue
					if (Random.value < advanceChance)
						continue;
					else
					{
						// If we didn't we need to evaluate wheter the worker can continue or not
						// We need to remove this direction from the future choices in this shift
						// If there's none left then the worker will retire
						possibleChoices.Remove(direction);
						isMoveValid = false;
					}
				}
			}
			else
			{
				// We have no more choices to make
				// This worker is basically done
				_lifespan = 0;
				// Exit by just saying the shif is not valid
				// This will prompt the logic to ignore it and the worker will be retired
				return new MazeShift { isValid = false };
			}
			// If the move was validated and we reached this point, then we will exit the loop and dig
		}
		// Update the position
		_position.Move(direction);
		// Should we decrease the lifespan?
		if (Random.value > SECOND_WIND_CHANCE)
			_lifespan--;
		// Return the result from our shift
		return new MazeShift
		{
			isValid = true,

			from = startPosition,
			to = _position,

			heading = direction
		};
	}

	public void ForceRetirement()
		=> _lifespan = 0;

	public static int RandomLifespan(int size)
		=> Random.Range(size / 2, size + 1);
}
