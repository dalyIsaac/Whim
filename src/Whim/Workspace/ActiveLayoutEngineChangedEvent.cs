using System;

namespace Whim;

/// <summary>
/// Event for when the active layout engine has changed.
/// </summary>
public class ActiveLayoutEngineChangedEventArgs : EventArgs
{
	public ILayoutEngine PreviousLayoutEngine { get; }
	public ILayoutEngine CurrentLayoutEngine { get; }

	public ActiveLayoutEngineChangedEventArgs(ILayoutEngine previous, ILayoutEngine current)
	{
		PreviousLayoutEngine = previous;
		CurrentLayoutEngine = current;
	}
}
