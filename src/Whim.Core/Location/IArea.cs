namespace Whim.Core.Location;

/// <summary>
/// The width and height of an item.
/// </summary>
public interface IArea
{
	/// <summary>
	/// The width of the item, in pixels.
	/// </summary>
	public int Width { get; }

	/// <summary>
	/// The height of the item, in pixels.
	/// </summary>
	public int Height { get; }
}
