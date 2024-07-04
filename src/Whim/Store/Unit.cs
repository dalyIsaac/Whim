namespace Whim;

/// <summary>
/// An empty type for <see cref="Result{T}"/>s which don't return anything.
/// </summary>
public record Unit()
{
	/// <summary>
	/// Default placeholder for empty results.
	/// </summary>
	public static Result<Unit> Result { get; } = DotNext.Result.FromValue(new Unit());
}
