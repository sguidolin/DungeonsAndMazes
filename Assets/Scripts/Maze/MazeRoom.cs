using Unity.Burst.Intrinsics;

public struct MazeRoom
{
	private MazeDirection _entrances;

	public byte Value => (byte)_entrances;

	public void CreateOpening(MazeDirection direction)
		=> _entrances |= direction;

#if UNITY_EDITOR
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
#endif
}
