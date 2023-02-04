using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A matcher is used by the command palette to find commands that match a given input.
/// </summary>
public interface IMatcher<T>
{
	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<IVariantRowModel<T>> Match(string query, IReadOnlyList<IVariantRowModel<T>> items);

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="IMatcher{T}"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(IVariantRowModel<T> item);
}
