using System.ComponentModel;

namespace Whim.CommandPalette;

/// <summary>
/// A view model for a single row in a variant.
/// </summary>
public interface IVariantRowViewModel<T> : IVariantRowModel<T>, INotifyPropertyChanged
{
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
