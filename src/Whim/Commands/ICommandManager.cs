using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// ICommandManager is responsible for managing all the commands for Whim.
/// This involves handling keybindings, dispatching commands, and providing
/// access to the commands themselves.
/// </summary>
public interface ICommandManager : ICommandItems, IDisposable
{
	/// <summary>
	/// Initialize the keyboard hook.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Loads the given commmands and keybinds.
	/// </summary>
	/// <param name="commands"></param>
	public void LoadCommands(IEnumerable<(ICommand, IKeybind?)> commands);
}
