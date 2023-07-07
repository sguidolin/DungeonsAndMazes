public class MazeNavigationTile
{
	public int cost;
	public int distance;
	public MazePosition position;
	public MazeNavigationTile parent;

	public int Weight => cost + distance;

	public void SetDistance(MazePosition target)
		=> distance = MazePosition.Distance(target, position);
}
