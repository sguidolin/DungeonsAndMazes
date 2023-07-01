[System.Flags]
public enum MazeDirection : byte
{
	North = 1 << 0,
	South = 1 << 1,
	West = 1 << 2,
	East = 1 << 3
}
