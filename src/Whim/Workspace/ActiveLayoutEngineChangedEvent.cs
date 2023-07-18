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
	public required IWorkspace Workspace { get; init; }

	/// <summary>
	/// The previous active layout engine.
	/// </summary>
	public required IImmutableLayoutEngine PreviousLayoutEngine { get; init; }

	/// <summary>
	/// The new active layout engine.
	/// </summary>
	public required IImmutableLayoutEngine CurrentLayoutEngine { get; init; }
}
