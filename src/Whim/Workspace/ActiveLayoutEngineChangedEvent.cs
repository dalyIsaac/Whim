using System;

namespace Whim;

/// <summary>
/// Event for when the active layout engine for a workspace has changed.
/// </summary>
public class ActiveLayoutEngineChangedEventArgs : EventArgs
{
	/// <summary>
	/// The workspace that has a new active layout engine.
	/// </summary>
	public IWorkspace Workspace { get; }

	/// <summary>
	/// The previous active layout engine.
	/// </summary>
	public ILayoutEngine PreviousLayoutEngine { get; }

	/// <summary>
	/// The new active layout engine.
	/// </summary>
	public ILayoutEngine CurrentLayoutEngine { get; }

	/// <summary>
	/// Constructs a new event args for when the active layout engine for a workspace has changed.
	/// </summary>
	/// <param name="workspace">The workspace that has a new active layout engine.</param>
	/// <param name="previous">The previous active layout engine.</param>
	/// <param name="current">The new active layout engine.</param>
	public ActiveLayoutEngineChangedEventArgs(IWorkspace workspace, ILayoutEngine previous, ILayoutEngine current)
	{
		Workspace = workspace;
		PreviousLayoutEngine = previous;
		CurrentLayoutEngine = current;
	}
}
