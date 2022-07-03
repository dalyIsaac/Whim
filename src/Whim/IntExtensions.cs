namespace Whim;

/// <summary>
/// Extension methods for <see cref="int"/>.
/// </summary>
public static class IntExtensions
{
	/// <summary>
	/// Modulus operator that returns the remainder of the division of the two operands.
	/// </summary>
	public static int Mod(this int value, int mod) => ((value % mod) + mod) % mod;
}
