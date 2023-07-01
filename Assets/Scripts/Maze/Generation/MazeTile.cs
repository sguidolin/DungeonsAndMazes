using UnityEngine;

[System.Serializable]
public struct MazeTile
{
	[SerializeField]
	private MazeDirection _entrances;

	public byte Value => (byte)_entrances;
	public string Hex => ((byte)_entrances).ToString("X");

	public void CreateOpening(MazeDirection direction)
		=> _entrances |= direction;

	public static MazeDirection Block => (MazeDirection)0;
	public static MazeDirection Open => MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.West;

	public static implicit operator MazeDirection(MazeTile t)
		=> (MazeDirection)t.Value;
	public static implicit operator MazeTile(MazeDirection d)
	{
		MazeTile tile = new MazeTile();
		tile.CreateOpening(d);
		return tile;
	}

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
			case MazeDirection.North | MazeDirection.West:
				unicode = (char)0x255D;
				break;
			case MazeDirection.North | MazeDirection.East:
				unicode = (char)0x255A;
				break;
			case MazeDirection.North | MazeDirection.South:
				unicode = (char)0x2551;
				break;
			case MazeDirection.South | MazeDirection.West:
				unicode = (char)0x2557;
				break;
			case MazeDirection.South | MazeDirection.East:
				unicode = (char)0x2554;
				break;
			case MazeDirection.West | MazeDirection.East:
				unicode = (char)0x2550;
				break;
			case MazeDirection.North | MazeDirection.West | MazeDirection.East:
				unicode = (char)0x2569;
				break;
			case MazeDirection.South | MazeDirection.West | MazeDirection.East:
				unicode = (char)0x2566;
				break;
			case MazeDirection.North | MazeDirection.South | MazeDirection.West:
				unicode = (char)0x2563;
				break;
			case MazeDirection.North | MazeDirection.South | MazeDirection.East:
				unicode = (char)0x2560;
				break;
			case MazeDirection.North | MazeDirection.South | MazeDirection.West | MazeDirection.East:
				unicode = (char)0x256C;
				break;
			default:
				unicode = (char)0x2592;
				break;
		}
		return unicode.ToString();
	}
}
