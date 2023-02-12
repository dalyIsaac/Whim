using System.ComponentModel;

namespace Whim.CommandPalette;

/// <summary>
/// A view model for a single row in a variant.
/// </summary>
public interface IVariantRowViewModel<T> : INotifyPropertyChanged
{
	/// <summary>
	/// The model for the palette item.
	/// </summary>
	/// <remarks>
	/// Do not bind to this property. This is only for <see cref="IVariantViewModel"/>
	/// implementations to use.
	///
	/// Similarly, do not update this property. Instead, use <see cref="Update"/>.
	/// </remarks>
	IVariantRowModel<T> Model { get; }

	/// <summary>
	/// The formatted title of the palette item.
	/// </summary>
	PaletteText FormattedTitle { get; }

	/// <summary>
	/// Updates the view model with the result from the matcher.
	/// </summary>
	/// <param name="matcherResult">The new result.</param>
	void Update(MatcherResult<T> matcherResult);
}
