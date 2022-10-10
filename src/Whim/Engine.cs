namespace Whim;

/// <summary>
/// Exposes the Whim <see cref="IConfigContext"/>.
/// </summary>
public static class Engine
{
	private static IConfigContext? _configContext;

	/// <summary>
	/// Get the <see cref="IConfigContext"/>.
	/// </summary>
	/// <returns>The <see cref="IConfigContext"/>.</returns>
	public static IConfigContext CreateConfigContext()
	{
		_configContext ??= new ConfigContext();

		return _configContext;
	}
}
