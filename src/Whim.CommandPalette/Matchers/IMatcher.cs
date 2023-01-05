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
	public IEnumerable<IVariantItem<T>> Match(string query, IReadOnlyList<IVariantItem<T>> items);

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="IMatcher{T}"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(IVariantItem<T> item);
}
