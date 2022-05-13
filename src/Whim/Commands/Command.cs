using System;

namespace Whim;

public class Command : ICommand
{
	public string Identifier { get; }

	public string Title { get; }

	public Action Callback { get; }

	public Action? Condition { get; }

	public Command(string identifier, string title, Action callback, Action? condition = null)
	{
		Identifier = identifier;
		Title = title;
		Callback = callback;
		Condition = condition;
	}

	public override string ToString()
	{
		return $"{Identifier} ({Title})";
	}
}
