using System;

namespace Whim;

/// <summary>
/// A direction in the coordinate space.
/// </summary>
[Flags]
public enum Direction
{
	/// <summary>
	/// No direction.
	/// </summary>
	None = 0,

	/// <summary>
	/// The left direction.
	/// </summary>
	Left = 1,

	/// <summary>
	/// The right direction.
	/// </summary>
	Right = 2,

	/// <summary>
	/// The up direction.
	/// </summary>
	Up = 4,

	/// <summary>
	/// The down direction.
	/// </summary>
	Down = 8,

	/// <summary>
	/// The left + up direction.
	/// </summary>
	LeftUp = Left | Up,

	/// <summary>
	/// The left + down direction.
	/// </summary>
	LeftDown = Left | Down,

	/// <summary>
	/// The right + up direction.
	/// </summary>
	RightUp = Right | Up,

	/// <summary>
	/// The right + down direction.
	/// </summary>
	RightDown = Right | Down,
}

/// <summary>
/// Extension methods for the <see cref="Direction"/> enum.
/// </summary>
public static class DirectionHelpers
{
	/// <summary>
	/// Returns true when the given direction is horizontal. Otherwise, returns false.
	/// </summary>
	public static bool IsHorizontal(this Direction direction) =>
		direction == Direction.Left || direction == Direction.Right;

	/// <summary>
	/// Returns the number of directions in the given direction.
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static int GetDirectionsCount(this Direction direction)
	{
		int count = 0;
		if (direction.HasFlag(Direction.Left))
		{
			count++;
		}
		if (direction.HasFlag(Direction.Right))
		{
			count++;
		}
		if (direction.HasFlag(Direction.Up))
		{
			count++;
		}
		if (direction.HasFlag(Direction.Down))
		{
			count++;
		}
		return count;
	}
}
