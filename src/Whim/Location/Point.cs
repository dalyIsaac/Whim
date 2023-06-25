using System.Numerics;

namespace Whim;

/// <inheritdoc />
public record Point<T> : IPoint<T>
	where T : INumber<T>
{
	/// <inheritdoc />
	public T X { get; init; } = T.Zero;

	/// <inheritdoc />
	public T Y { get; init; } = T.Zero;

	/// <inheritdoc />
	public override string ToString() => $"({X}, {Y})";
}
