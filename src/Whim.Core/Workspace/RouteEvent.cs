using System;

namespace Whim.Core;

/// <summary>
/// Describes how an <see cref="IWindow"/> has been routed between workspaces.
/// This implicitly describes hypothetical `WindowAdded` and `WindowRemoved` events
/// for a workspace.
/// </summary>
public class RouteEventArgs : EventArgs
{
	public IWindow Window { get; }

	/// <summary>
	/// The workspace that the window was routed from. If the window has just
	/// been registered, this will be null.
	/// </summary>
	public IWorkspace? FromWorkspace { get; }

	/// <summary>
	/// The workspace that the window was routed to. If the window has just
	/// been unregistered, this will be null.
	public IWorkspace? ToWorkspace { get; }

	private RouteEventArgs(IWindow window, IWorkspace? fromWorkspace, IWorkspace? toWorkspace)
	{
		Window = window;
		FromWorkspace = fromWorkspace;
		ToWorkspace = toWorkspace;
	}

	public static RouteEventArgs WindowAdded(IWindow window, IWorkspace workspace) => new(window, null, workspace);

	public static RouteEventArgs WindowRemoved(IWindow window, IWorkspace workspace) => new(window, workspace, null);

	public static RouteEventArgs WindowMoved(IWindow window, IWorkspace fromWorkspace, IWorkspace toWorkspace) => new(window, fromWorkspace, toWorkspace);
}
