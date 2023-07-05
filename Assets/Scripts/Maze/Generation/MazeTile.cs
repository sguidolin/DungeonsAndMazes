using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct MazeTile
{
	[SerializeField]
	private MazeDirection _entrances;

	public byte Value => (byte)_entrances;
	public string Hex => ((byte)_entrances).ToString("X");

	public int Openings
	{
		get
		{
			int openings = 0;
			foreach (MazeDirection direction in Cardinals)
				if ((direction & _entrances) != 0)
					openings++;
			return openings;
		}
	}
	public IEnumerable<MazeDirection> Entrances
	{
		get
		{
			foreach (MazeDirection direction in Cardinals)
				if ((direction & _entrances) != 0)
					yield return direction;
		}
	}
	public IEnumerable<MazeDirection> Walls
	{
		get
		{
			foreach (MazeDirection direction in Cardinals)
				if ((direction & _entrances) == 0)
					yield return direction;
		}
	}

	public void CreateOpening(MazeDirection direction)
		=> _entrances |= direction;

	public MazeDirection GetRandomDig()
	{
		MazeDirection[] directions = Walls.ToArray<MazeDirection>();
		return directions[Random.Range(0, directions.Length)];
	}


	public static MazeDirection Block
		=> (MazeDirection)0;
	public static MazeDirection Open
		=> MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.West;
	public static MazeDirection[] Cardinals =>
		new MazeDirection[]
		{
			MazeDirection.North, MazeDirection.South, MazeDirection.West, MazeDirection.East
		};

	#region Operators Overloads
	public static bool operator ==(MazeTile current, MazeTile other)
		=> current.Value == other.Value;
	public static bool operator !=(MazeTile current, MazeTile other)
		=> !(current == other);

	public static implicit operator MazeDirection(MazeTile t)
		=> (MazeDirection)t.Value;
	public static implicit operator MazeTile(MazeDirection d)
	{
		MazeTile tile = new MazeTile();
		tile.CreateOpening(d);
		return tile;
	}
	#endregion
	#region Methods Overrides
	public override bool Equals(object obj)
	=> base.Equals(obj);
	public override int GetHashCode()
		=> base.GetHashCode();

	public override string ToString()
	{
		char unicode = '\0';
		switch (_entrances)
		{
			case MazeDirection.North:
				unicode = (char)0x2568;
				break;
			case MazeDirection.South:
				unicode = (char)0x2565;
				break;
			case MazeDirection.West:
				unicode = (char)0x2561;
				break;
			case MazeDirection.East:
				unicode = (char)0x255E;
				break;
			case MazeDirection.NorthWest:
				unicode = (char)0x255D;
				break;
			case MazeDirection.NorthEast:
				unicode = (char)0x255A;
				break;
			case MazeDirection.NorthSouth:
				unicode = (char)0x2551;
				break;
			case MazeDirection.SouthWest:
				unicode = (char)0x2557;
				break;
			case MazeDirection.SouthEast:
				unicode = (char)0x2554;
				break;
			case MazeDirection.WestEast:
				unicode = (char)0x2550;
				break;
			case MazeDirection.NorthWestEast:
				unicode = (char)0x2569;
				break;
			case MazeDirection.SouthWestEast:
				unicode = (char)0x2566;
				break;
			case MazeDirection.NorthSouthWest:
				unicode = (char)0x2563;
				break;
			case MazeDirection.NorthSouthEast:
				unicode = (char)0x2560;
				break;
			case MazeDirection.Compass:
				unicode = (char)0x256C;
				break;
			default:
				unicode = (char)0x2592;
				break;
		}
		return unicode.ToString();
	}
	#endregion
}
