using System.Linq;
using Xunit;

namespace Whim.TestUtilities;

/// <summary>
/// Test utilities for <see cref="_pluginCommands"/>.
/// </summary>
public class PluginCommandsTestUtils
{
	private readonly PluginCommands _pluginCommands;

	/// <summary>
	/// Create the plugin commands test utilities.
	/// </summary>
	/// <param name="pluginCommands"></param>
	public PluginCommandsTestUtils(PluginCommands pluginCommands)
	{
		_pluginCommands = pluginCommands;
	}

	/// <summary>
	/// Get the command with the given identifier.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public ICommand GetCommand(string id)
	{
		ICommand? command = _pluginCommands.Commands.FirstOrDefault(c => c.Id == id);
		Assert.NotNull(command);
		return command!;
	}
}
