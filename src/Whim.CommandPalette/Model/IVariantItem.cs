namespace Whim.CommandPalette;

/// <summary>
/// An item displayed by a <see cref="IVariantViewModel{T}"/>.
/// </summary>
public interface IVariantItem<T>
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
	PaletteText FormattedTitle { get; set; }

	/// <summary>
	/// The underlying data.
	/// </summary>
	T Data { get; }
}
