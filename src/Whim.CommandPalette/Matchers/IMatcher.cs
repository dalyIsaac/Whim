using System.Collections.Generic;
using System.Text.Json;

namespace Whim.CommandPalette;

/// <summary>
/// A matcher is used by the command palette to find <template name="TData"/>  that match a given input.
/// </summary>
/// <typeparam name="TData">The type of data the matcher matches.</typeparam>
public interface IMatcher<TData>
{
	/// <summary>
	/// Matcher returns an ordered list of filtered matches for the <paramref name="query"/>.
	/// </summary>
	public IEnumerable<MatcherResult<TData>> Match(string query, IReadOnlyList<IVariantRowModel<TData>> items);

	/// <summary>
	/// Called when a match has been executed. This is used by the <see cref="IMatcher{TData}"/>
	/// implementation to update relevant internal state.
	/// </summary>
	public void OnMatchExecuted(IVariantRowModel<TData> item);

	/// <summary>
	/// Load the matcher's state from <paramref name="state"/>.
	/// </summary>
	/// <param name="state">The matcher's state.</param>
	void LoadState(JsonElement state);

	/// <summary>
	/// Save the matcher's state as a <see cref="JsonElement"/>.
	/// </summary>
	/// <returns>The matcher's state.</returns>
	JsonElement? SaveState();
}
