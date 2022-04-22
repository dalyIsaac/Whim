using System;

namespace Whim;

public class CommandEventArgs : EventArgs
{
	public ICommand Command { get; set; }

	public CommandEventArgs(ICommand command)
	{
		Command = command;
	}
}
