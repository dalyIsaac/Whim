namespace Whim.CommandPalette;

/// <summary>
/// A control that represents a single row in a variant.
/// </summary>
/// <typeparam name="T">The variant item's data type.</typeparam>
/// <typeparam name="TVM">The variant item's view model type.</typeparam>
public interface IVariantRowControl<T, TVM> where TVM : IVariantRowViewModel<T>
{
	/// <summary>
	/// The item displayed by this row.
	/// </summary>
	public TVM ViewModel { get; }

	/// <summary>
	/// Initializes the row. For example, sets the title.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Updates the row with the result from the matcher.
	/// </summary>
	/// <param name="matcherResult">The new result.</param>
	public void Update(MatcherResult<T> matcherResult);
}
