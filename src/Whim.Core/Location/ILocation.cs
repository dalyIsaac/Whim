namespace Whim.Core.Location;

/// <summary>
/// The location of an item, where the origin is in the top-left of the primary monitor.
/// </summary>
public interface ILocation : IArea
{
	/// <summary>
	/// The x-coordinate of the left of the item.
	/// </summary>
	public int X { get; }

	/// <summary>
	/// The y-coordinate of the top of the item.
	/// </summary>
	public int Y { get; }

	/// <summary>
	/// Indicates whether the specified point is inside this item's bounding box.
	/// </summary>
	public bool IsPointInside(int x, int y);
}
