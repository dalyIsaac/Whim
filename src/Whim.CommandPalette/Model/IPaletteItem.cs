namespace Whim.CommandPalette;

/// <summary>
/// A palette item.
/// </summary>
public interface IPaletteItem<T>
{
	/// <summary>
	/// The unique identifier of the palette item.
	/// </summary>
	string Id { get; }

	/// <summary>
	/// The title of the palette item.
	/// </summary>
	string Title { get; }

	/// <summary>
	/// The formatted title of the palette item.
	/// </summary>
	Text FormattedTitle { get; set; }

	/// <summary>
	/// The underlying item.
	/// </summary>
	T Item { get; }
}
