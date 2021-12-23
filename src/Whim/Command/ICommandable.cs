namespace Whim;

/// <summary>
/// Specification for classes to handle and expose command functionality.
/// </summary>
public interface ICommandable
{
	/// <summary>
	/// <see cref="Commander"/> exposes command functionality.
	/// </summary>
	public Commander Commander { get; }
}
