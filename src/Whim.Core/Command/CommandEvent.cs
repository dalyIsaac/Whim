using System;

namespace Whim.Core.Command;

public delegate void CommandHandlerDelegate(object sender, CommandEventArgs args);

public class CommandEventArgs : EventArgs
{
	public ICommand Command { get; set; }

	public CommandEventArgs(ICommand command)
	{
		Command = command;
	}
}
