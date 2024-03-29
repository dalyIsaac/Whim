using System;

namespace Whim;

/// <summary>
/// Describes how an <see cref="IWindow"/> has been routed between workspaces.
/// This implicitly describes hypothetical `WindowAdded` and `WindowRemoved` events
/// for a workspace.
/// </summary>
public class RouteEventArgs : EventArgs
{
	/// <summary>
	/// The window that was routed.
	/// </summary>
	public IWindow Window { get; }

	/// <summary>
	/// The workspace that the window was routed from. If the window has just
	/// been added, this will be null.
	/// </summary>
	public IWorkspace? PreviousWorkspace { get; }

	/// <summary>
	/// The workspace that the window was routed to. If the window has just
	/// been removed, this will be null.
	/// </summary>
	public IWorkspace? CurrentWorkspace { get; }

	private RouteEventArgs(IWindow window, IWorkspace? previousWorkspace, IWorkspace? currentWorkspace)
	{
		Window = window;
		PreviousWorkspace = previousWorkspace;
		CurrentWorkspace = currentWorkspace;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj is not RouteEventArgs other)
		{
			return false;
		}

		return other.Window.Equals(Window)
			&& other.PreviousWorkspace == PreviousWorkspace
			&& other.CurrentWorkspace == CurrentWorkspace;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Window, PreviousWorkspace, CurrentWorkspace);

	/// <summary>
	/// Helper method for creating a new <see cref="RouteEventArgs"/> for when a window is added to a workspace.
	/// </summary>
	public static RouteEventArgs WindowAdded(IWindow window, IWorkspace workspace) => new(window, null, workspace);

	/// <summary>
	/// Helper method for creating a new <see cref="RouteEventArgs"/> for when a window is removed from a workspace.
	/// </summary>
	public static RouteEventArgs WindowRemoved(IWindow window, IWorkspace workspace) => new(window, workspace, null);

	/// <summary>
	/// Helper method for creating a new <see cref="RouteEventArgs"/> for when a window is routed between workspaces.
	/// </summary>
	public static RouteEventArgs WindowMoved(IWindow window, IWorkspace fromWorkspace, IWorkspace toWorkspace) =>
		new(window, fromWorkspace, toWorkspace);
}
