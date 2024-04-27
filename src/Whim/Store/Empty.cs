using DotNext;

namespace Whim;

/// <summary>
/// An empty type for <see cref="Result{T}"/>s which don't return anything.
/// </summary>
public record Empty()
{
	/// <summary>
	/// Default placeholder for empty result.
	/// </summary>
	public static Result<Empty> Result { get; } = DotNext.Result.FromValue(new Empty());
}
