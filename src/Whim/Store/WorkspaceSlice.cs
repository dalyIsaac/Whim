using System;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The slice containing workspaces.
/// </summary>
public class WorkspaceSlice : ISlice
{
	/// <summary>
	/// All the workspaces currently tracked by Whim.
	/// </summary>
	public ImmutableList<IWorkspace> Workspaces { get; } = ImmutableList<IWorkspace>.Empty;

	/// <summary>
	/// The index of the workspace which is currently active, in <see cref="Workspaces"/>.
	/// </summary>
	public int ActiveWorkspaceIndex { get; } = -1;

	// TODO: Add to StoreTests
	internal override void Initialize() { }

	internal override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				// TODO
				default:
					break;
			}
		}

		_events.Clear();
	}
}
