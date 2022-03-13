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
	public static bool IsHorizontal(this Direction direction)
	{
		return direction == Direction.Left || direction == Direction.Right;
	}
}
