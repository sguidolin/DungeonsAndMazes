using UnityEngine;

public struct MazePosition
{
	public int x;
	public int y;

	public MazePosition(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public void Move(MazeDirection direction)
	{
		switch (direction)
		{
			case MazeDirection.North:
				x--;
				break;
			case MazeDirection.South:
				x++;
				break;
			case MazeDirection.West:
				y--;
				break;
			case MazeDirection.East:
				y++;
				break;
		}
	}

	public static implicit operator Vector2(MazePosition p) => new Vector2(p.x, p.y);
	public static implicit operator MazePosition(Vector2 v) => new MazePosition((int)v.x, (int)v.y);

	public override string ToString() => $"({x}, {y})";
}
