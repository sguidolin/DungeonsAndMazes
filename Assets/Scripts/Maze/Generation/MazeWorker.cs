using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeWorker
{
	private const float BIAS_FACTOR = 0.215f;
	private const float INDECISIVENESS_FACTOR = 0.45f;
	private const float SECOND_WIND_CHANCE = 0.25f;

	private readonly MazeDirection[] WORK_CHOICES = new MazeDirection[]
	{
		MazeDirection.North, MazeDirection.South, MazeDirection.West, MazeDirection.East
	};

	private MazePosition _position;
	private int _lifespan;

	// Random factors
	private float _bias;
	private float _indecisiveness;

	private bool _smartDigging = false;
	private List<MazeDirection> _choices;

	public int Lifespan => _lifespan;
	public bool Retired => !(_lifespan > 0);
	public MazePosition Position => _position;

	public MazeWorker(bool useSmartDigging, int lifespan, MazePosition deployment)
	{
		_smartDigging = useSmartDigging;

		_lifespan = lifespan;
		_position = deployment;
		// How indecisive is our worker?
		_indecisiveness = Random.value * INDECISIVENESS_FACTOR;
		// What's the chance for bias of our worker?
		_bias = Random.value * BIAS_FACTOR;
		// Initialize the choices the worker can make
		_choices = new List<MazeDirection>(WORK_CHOICES);
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

	private bool IsLegalMove(MazeDirection direction, int minDepth, int maxDepth, int minWidth, int maxWidth)
	{
		// Evaluate the border cases
		if (direction == MazeDirection.North && _position.x == minDepth)
			return false;
		if (direction == MazeDirection.South && _position.x == maxDepth)
			return false;
		if (direction == MazeDirection.West && _position.y == minWidth)
			return false;
		if (direction == MazeDirection.East && _position.y == maxWidth)
			return false;
		return true;
	}
	private bool HasLegalMoves(List<MazeDirection> choices, int minDepth, int maxDepth, int minWidth, int maxWidth)
		=> choices.Any<MazeDirection>(dir => IsLegalMove(dir, minDepth, maxDepth, minWidth, maxWidth));

	public MazeShift Work(MazeGrid locale)
	{
		// Set up boundaries data
		int minDepth = 0, maxDepth = locale.Depth - 1, minWidth = 0, maxWidth = locale.Width - 1;

		bool isMoveValid = false;
		MazeDirection direction = 0;
		MazePosition startPosition = _position;
		// Instantiate a new list of choices
		List<MazeDirection> possibleChoices = _choices;

		while (!isMoveValid)
		{
			// Make sure the worker has legal choices to make
			if (HasLegalMoves(possibleChoices, minDepth, maxDepth, minWidth, maxWidth))
			{
				// Let the worker take longer if they're too indecisive
				while (Random.value < _indecisiveness)
					direction = possibleChoices[Random.Range(0, possibleChoices.Count)];
				// Working is mandatory
				if (direction == 0)
					continue;
				// Check validity of the move we picked
				isMoveValid = IsLegalMove(direction, minDepth, maxDepth, minWidth, maxWidth);
				// If we can move, we need to evaluate if we actually want to go through
				// This is based on the number of holes that have been dug already in the next cell
				if (isMoveValid && _smartDigging)
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
					float advanceChance = 1f / (holes * 1.25f);
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
}
