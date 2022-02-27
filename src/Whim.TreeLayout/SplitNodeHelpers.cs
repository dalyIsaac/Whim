namespace Whim.TreeLayout;

internal static class SplitNodeHelpers
{
	/// <summary>
	/// Returns <see langword="true"/> if the direction indicates that a
	/// newly added node would be placed after the currently focused node.
	/// </summary>
	internal static bool IsPositiveIndex(this Direction direction)
	{
		return direction == Direction.Right || direction == Direction.Down;
	}
}
