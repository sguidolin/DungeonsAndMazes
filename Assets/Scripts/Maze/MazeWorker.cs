using UnityEngine;

public class MazeWorker
{
	private const float INDECISIVENESS_FACTOR = 0.45f;
	private const float SECOND_WIND_CHANCE = 0.25f;

	private readonly MazeDirection[] _choices = new MazeDirection[]
	{
		MazeDirection.North, MazeDirection.South, MazeDirection.West, MazeDirection.East
	};

	private int _lifespan;
	private float _indecisiveness;
	private MazePosition _position;

	public int Lifespan => _lifespan;
	public bool Retired => !(_lifespan > 0);
	public MazePosition Position => _position;

	public MazeWorker(int lifespan, MazePosition deployment)
	{
		_lifespan = lifespan;
		_position = deployment;
		// How indecisive is our worker?
		_indecisiveness = Random.value * INDECISIVENESS_FACTOR;
	}

	public MazeShift Work(MazeGrid locale)
	{
		bool isMoveValid = false;
		MazeDirection direction = 0;
		MazePosition startPosition = _position;

		while (!isMoveValid)
		{
			// Let the worker take longer if they're too indecisive
			while (Random.value < _indecisiveness)
				direction = _choices[Random.Range(0, _choices.Length)];
			// Working is mandatory
			if (direction == 0)
				continue;
			// Evaluate the border cases
			if (direction == MazeDirection.North && _position.x == 0)
				continue;
			if (direction == MazeDirection.South && _position.x == locale.Depth - 1)
				continue;
			if (direction == MazeDirection.West && _position.y == 0)
				continue;
			if (direction == MazeDirection.East && _position.y == locale.Width - 1)
				continue;
			// If we got here, then the move is valid and we can dig
			isMoveValid = true;
		}
		// Update the position
		_position.Move(direction);
		// Should we decrease the lifespan?
		if (Random.value > SECOND_WIND_CHANCE)
			_lifespan--;
		// Return the result from our shift
		return new MazeShift
		{
			from = startPosition,
			to = _position,

			heading = direction
		};
	}
}
