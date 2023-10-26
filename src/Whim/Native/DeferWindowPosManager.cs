using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Whim;

internal class DeferWindowPosManager : IDeferWindowPosManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	private readonly Dictionary<IWindow, WindowPosState> _deferredWindowStates = new();

	public ParallelOptions ParallelOptions { get; } = new();

	public DeferWindowPosManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public void DeferLayout(List<WindowPosState> windowStates)
	{
		Logger.Debug("Deferring layout");
		foreach (WindowPosState windowState in windowStates)
		{
			_deferredWindowStates[windowState.WindowState.Window] = windowState;
		}
	}

	public bool CanDoLayout()
	{
		StackTrace stackTrace = new();

		// Iterate over each of the stack trace to see if reentrancy has occurred.
		// If so, we cannot do layout.
		int entryCount = 0;
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame? frame = stackTrace.GetFrame(i);
			if (frame?.GetMethod() == typeof(WindowManager).GetMethod(nameof(WindowManager.WindowsEventHook)))
			{
				entryCount++;

				if (entryCount > 1)
				{
					Logger.Debug("Cannot do layout due to reentrancy");
					return false;
				}
			}
		}

		return true;
	}

	public void RecoverLayout()
	{
		Logger.Debug("Attempting to recover layout");

		if (!CanDoLayout())
		{
			Logger.Debug("Cannot recover layout");
			return;
		}
		else if (_deferredWindowStates.Count == 0)
		{
			Logger.Debug("No windows to recover layout for");
			return;
		}

		List<IWorkspace> deferredWorkspaces = new();
		List<WindowPosState> deferredWindowStates = new();
		foreach (WindowPosState windowState in _deferredWindowStates.Values)
		{
			IWorkspace? workspace = _context.WorkspaceManager.GetWorkspaceForWindow(windowState.WindowState.Window);
			if (workspace != null && !deferredWorkspaces.Contains(workspace))
			{
				deferredWorkspaces.Add(workspace);
			}
			else if (_context.FilterManager.ShouldBeIgnored(windowState.WindowState.Window))
			{
				// We don't store the window in the window manager, but we do still set its position.
				// We only set the positions of windows which aren't tracked by Whim - the other windows
				// will be set out by their respective workspaces.
				deferredWindowStates.Add(windowState);
			}
		}

		foreach (IWorkspace workspace in deferredWorkspaces)
		{
			workspace.DoLayout();
		}

		if (deferredWindowStates.Count != 0)
		{
			using DeferWindowPosHandle windowDeferPosHandle = new(_context, _internalContext, deferredWindowStates);
		}

		_deferredWindowStates.Clear();
	}
}
