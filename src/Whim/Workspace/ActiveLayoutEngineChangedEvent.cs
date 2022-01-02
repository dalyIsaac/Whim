using System;

namespace Whim;

/// <summary>
/// Event for when the active layout engine has changed.
/// </summary>
public class ActiveLayoutEngineChangedEventArgs : EventArgs
{
	public IWorkspace Workspace { get; }
	public ILayoutEngine PreviousLayoutEngine { get; }
	public ILayoutEngine CurrentLayoutEngine { get; }

	public ActiveLayoutEngineChangedEventArgs(IWorkspace workspace, ILayoutEngine previous, ILayoutEngine current)
	{
		Workspace = workspace;
		PreviousLayoutEngine = previous;
		CurrentLayoutEngine = current;
	}
}
