namespace Whim;

/// <summary>
/// Exposes the Whim <see cref="IContext"/>.
/// </summary>
public static class Engine
{
	private static IContext? _context;

	/// <summary>
	/// Get the <see cref="IContext"/>.
	/// </summary>
	/// <returns>The <see cref="IContext"/>.</returns>
	public static IContext CreateContext()
	{
		_context ??= new Context();

		return _context;
	}
}
