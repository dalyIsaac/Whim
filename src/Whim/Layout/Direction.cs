namespace Whim;

/// <summary>
/// A direction in the coordinate space.
/// </summary>
public enum Direction
{
	/// <summary>
	/// The up direction.
	/// </summary>
	Up,

	/// <summary>
	/// The down direction.
	/// </summary>
	Down,

	/// <summary>
	/// The left direction.
	/// </summary>
	Left,

	/// <summary>
	/// The right direction.
	/// </summary>
	Right,
}

/// <summary>
/// Extension methods for the <see cref="Direction"/> enum.
/// </summary>
public static class DirectionHelpers
{
	/// <summary>
	/// Returns true when the given direction is horizontal. Otherwise, returns false.
	/// </summary>
	public static bool IsHorizontal(this Direction direction) => direction is Direction.Left or Direction.Right;
}
