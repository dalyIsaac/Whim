using System.Reflection;

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
	/// <param name="assembly">The calling assembly.</param>
	/// <returns>The <see cref="IConfigContext"/>.</returns>
	/// <exception cref="ConfigLoaderException"></exception>
	public static IConfigContext CreateConfigContext(Assembly? assembly)
	{
		if (_configContext == null)
		{
			if (assembly == null)
			{
				throw new ConfigLoaderException("Provided assembly was null.");
			}

			_configContext = new ConfigContext(assembly);
		}

		return _configContext;
	}
}
