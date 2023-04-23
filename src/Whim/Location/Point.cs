using System.Numerics;

namespace Whim;

/// <inheritdoc />
public record Point<T> : IPoint<T>
	where T : INumber<T>
{
	/// <inheritdoc />
	public required T X { get; init; }

	/// <inheritdoc />
	public required T Y { get; init; }

	/// <inheritdoc />
	public override string ToString() => $"({X}, {Y})";
}
