using System;

namespace Whim.Core;

/// <summary>
/// Event for when the active layout engine has changed.
/// </summary>
public class ActiveLayoutEngineChangedEventArgs : EventArgs
{
	public ILayoutEngine Previous { get; }
	public ILayoutEngine Current { get; }

	public ActiveLayoutEngineChangedEventArgs(ILayoutEngine previous, ILayoutEngine current)
	{
		Previous = previous;
		Current = current;
	}
}
