namespace Whim;

public enum Direction
{
	Up,
	Down,
	Left,
	Right,
}

public static class DirectionHelpers
{
	/// <summary>
	/// Returns true when the given direction is horizontal. Otherwise, returns false.
	/// </summary>
	public static bool IsHorizontal(this Direction direction) => direction is Direction.Left or Direction.Right;
}
