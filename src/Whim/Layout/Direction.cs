namespace Whim;

/// <summary>
/// A direction in the coordinate space.
/// </summary>
/// <remarks>
/// <c>Up</c> + <c>Right</c> would be <c>Up | Right == 0b1000 | 0b11 == 0b1011</c>.
/// It's not possible to have a direction that is both up and down, or left and right.
/// </remarks>
public enum Direction
{
	/// <summary>
	/// No direction.
	/// </summary>
	None = 0,

	/// <summary>
	/// The left direction.
	/// </summary>
	Left = 0b10,

	/// <summary>
	/// The right direction.
	/// </summary>
	Right = 0b11,

	/// <summary>
	/// The up direction.
	/// </summary>
	Up = 0b1000,

	/// <summary>
	/// The down direction.
	/// </summary>
	Down = 0b1100,
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
