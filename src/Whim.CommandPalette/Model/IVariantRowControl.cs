namespace Whim.CommandPalette;

/// <summary>
/// A control that represents a single row in a variant.
/// </summary>
/// <typeparam name="T">The variant item's data type.</typeparam>
public interface IVariantRowControl<T>
{
	/// <summary>
	/// The item displayed by this row.
	/// </summary>
	public IVariantRowModel<T> Item { get; }

	/// <summary>
	/// Initializes the row. For example, sets the title.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Updates the row with a new item.
	/// </summary>
	/// <param name="item"></param>
	public void Update(IVariantRowModel<T> item);
}
