using System.Numerics;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="int"/>.
/// </summary>
public static class IntExtensions
{
	/// <summary>
	/// Modulus operator that returns the remainder of the division of the two operands.
	/// </summary>
	public static T Mod<T>(this T value, T mod)
		where T : INumber<T> => ((value % mod) + mod) % mod;
}
