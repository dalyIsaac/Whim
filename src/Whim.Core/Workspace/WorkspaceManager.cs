using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim.Core;

/// <summary>
/// Implementation of <see cref="IWorkspaceManager"/>.
/// </summary>
public class WorkspaceManager : IWorkspaceManager
{
	private readonly IConfigContext _configContext;
	public Commander Commander { get; } = new();

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	private readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// Maps windows to their workspace.
	/// </summary>
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace? ActiveWorkspace { get; private set; }

	public WorkspaceManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	public void Initialize()
	{
		_configContext.WindowManager.WindowRegistered += OnWindowRegistered;
	}

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public void Add(IWorkspace workspace)
	{
		_workspaces.Add(workspace);
	}

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool Remove(IWorkspace workspace)
	{
		Logger.Debug("Removing workspace {0}", workspace.Name);

		if (_workspaces.Count <= _configContext.MonitorManager.Length)
		{
			throw new InvalidOperationException($"There must be at least {_configContext.MonitorManager.Length} workspaces.");
		}

		bool wasFound = _workspaces.Remove(workspace);

		// Remap windows to the last workspace
		IWorkspace lastWorkspace = _workspaces[^1];

		foreach (IWindow window in workspace.Windows)
		{
			lastWorkspace.AddWindow(window);
			_windowWorkspaceMap[window] = lastWorkspace;
		}

		return wasFound;
	}

	public bool Remove(string workspaceName)
	{
		Logger.Debug("Trying to remove workspace {0}", workspaceName);

		IWorkspace? workspace = _workspaces.Find(w => w.Name == workspaceName);
		if (workspace == null)
		{
			Logger.Debug("Workspace {0} not found", workspaceName);
			return false;
		}

		return Remove(workspace);
	}

	public IWorkspace? TryGet(string workspaceName)
	{
		Logger.Debug("Trying to get workspace {0}", workspaceName);
		return _workspaces.Find(w => w.Name == workspaceName);
	}

	#region Windows
	internal void OnWindowRegistered(object sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		window.WindowUnregistered += OnWindowUnregistered;

		ActiveWorkspace?.AddWindow(window);
	}

	internal void OnWindowUnregistered(object sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		window.WindowUnregistered -= OnWindowUnregistered;

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error("Window {0} was not found in any workspace", window.Title);
			return;
		}

		workspace.RemoveWindow(window);
		_windowWorkspaceMap.Remove(window);
	}
	#endregion
}
