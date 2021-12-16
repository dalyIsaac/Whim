using System;

namespace Whim.Core;

/// <summary>
/// Event for when a layout engine has been added or removed from a workspace.
/// </summary>
public class LayoutEngineEventArgs : EventArgs
{
	public IWorkspace Workspace { get; }
	public ILayoutEngine LayoutEngine { get; }

	public LayoutEngineEventArgs(IWorkspace workspace, ILayoutEngine layoutEngine)
	{
		Workspace = workspace;
		LayoutEngine = layoutEngine;
	}
}
