namespace Whim.CommandPalette;

/// <summary>
/// Config for command palette variants which use a matcher.
/// </summary>
/// <typeparam name="TData">The type of data the matcher matches.</typeparam>
public interface IMatcherVariantConfig<TData>
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public IMatcher<TData> Matcher { get; }
}
