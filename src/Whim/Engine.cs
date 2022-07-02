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
	public static IConfigContext CreateConfigContext()
	{
		if (_configContext == null)
		{
			_configContext = new ConfigContext();
		}
		return _configContext;
	}
}
