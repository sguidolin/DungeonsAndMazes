using UnityEngine;

[System.Serializable]
public struct MazePosition
{
	[Min(0)] public int x;
	[Min(0)] public int y;

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

	public static MazePosition Move(MazePosition currentPosition, MazeDirection direction)
	{
		MazePosition nextPosition = currentPosition;
		//if (MazeGrid.Instance.IsMoveLegal(direction, nextPosition))
		nextPosition.Move(direction);
		return nextPosition;
	}
	public static int Distance(MazePosition from, MazePosition to)
		=> Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

	public Vector3 ToWorldPosition(float size) => new Vector3(-x * size, 0f, -y * size);

	#region Operator Overloads
	public static bool operator ==(MazePosition current, MazePosition other)
		=> current.x == other.x && current.y == other.y;
	public static bool operator !=(MazePosition current, MazePosition other)
		=> !(current == other);

	public static implicit operator Vector2(MazePosition p) => new Vector2(p.x, p.y);
	public static implicit operator MazePosition(Vector2 v) => new MazePosition((int)v.x, (int)v.y);
	#endregion
	#region Methods Overrides
	public override bool Equals(object obj)
		=> base.Equals(obj);
	public override int GetHashCode()
		=> base.GetHashCode();

	public override string ToString() => $"({x}, {y})";
	#endregion
}
