namespace Whim;

public static partial class Pickers
{
	/// <summary>
	/// Get the workspace with the provided <paramref name="workspaceId"/>.
	/// </summary>
	/// <param name="workspaceId"></param>
	/// <returns>
	/// The workspace with the provided <paramref name="workspaceId"/>, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceById(WorkspaceId workspaceId) =>
		(IRootSector rootSector) => BaseWorkspacePicker(workspaceId, rootSector, workspace => workspace);

	/// <summary>
	/// Get all workspaces.
	/// </summary>
	/// <returns>
	/// All workspaces, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<IEnumerable<IWorkspace>> PickWorkspaces() =>
		static (IRootSector rootSector) => GetAllActiveWorkspaces(rootSector.WorkspaceSector);

	private static IEnumerable<IWorkspace> GetAllActiveWorkspaces(IWorkspaceSector workspaceSector)
	{
		foreach (WorkspaceId id in workspaceSector.WorkspaceOrder)
		{
			yield return workspaceSector.Workspaces[id];
		}
	}

	/// <summary>
	/// Get the workspace with the provided <paramref name="name"/>.
	/// </summary>
	/// <param name="name"></param>
	/// <returns>
	/// The workspace with the provided <paramref name="name"/>, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceByName(string name) =>
		(IRootSector rootSector) =>
		{
			foreach (IWorkspace workspace in rootSector.WorkspaceSector.Workspaces.Values)
			{
				if (workspace.Name == name)
				{
					return Result.FromValue(workspace);
				}
			}

			return Result.FromError<IWorkspace>(new WhimError($"Workspace with name {name} not found"));
		};

	/// <summary>
	/// Base picker to get something from a workspace, provided by <paramref name="operation"/>.
	/// </summary>
	/// <param name="workspaceId">The id of the workspace to get something from.</param>
	/// <param name="rootSector">The root sector.</param>
	/// <param name="operation">The operation to determine what to get.</param>
	/// <typeparam name="TResult">The result.</typeparam>
	private static Result<TResult> BaseWorkspacePicker<TResult>(
		WorkspaceId workspaceId,
		IRootSector rootSector,
		Func<IWorkspace, TResult> operation
	)
	{
		if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace))
		{
			return Result.FromError<TResult>(StoreExceptions.WorkspaceNotFound(workspaceId));
		}

		return operation(workspace);
	}

	/// <summary>
	/// Base picker to get something from a workspace, provided by <paramref name="operation"/>.
	/// This operation returns a <see cref="Result{T}"/>.
	/// </summary>
	/// <param name="workspaceId">The id of the workspace to get something from.</param>
	/// <param name="rootSector">The root sector.</param>
	/// <param name="operation">
	/// The operation to determine what to get. This operation returns a <see cref="Result{T}"/>.
	/// </param>
	/// <typeparam name="TResult">The result.</typeparam>
	private static Result<TResult> BaseWorkspacePicker<TResult>(
		WorkspaceId workspaceId,
		IRootSector rootSector,
		Func<IWorkspace, Result<TResult>> operation
	)
	{
		if (workspaceId == default)
		{
			return operation(PickActiveWorkspace()(rootSector));
		}
		if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace))
		{
			return Result.FromError<TResult>(StoreExceptions.WorkspaceNotFound(workspaceId));
		}

		return operation(workspace);
	}

	/// <summary>
	/// Get the active workspace.
	/// </summary>
	/// <returns>
	/// The active workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<IWorkspace> PickActiveWorkspace() =>
		static (IRootSector rootSector) =>
			rootSector.WorkspaceSector.Workspaces[
				rootSector.MapSector.MonitorWorkspaceMap[rootSector.MonitorSector.ActiveMonitorHandle]
			];

	internal static Workspace PickMutableActiveWorkspace(MutableRootSector rootSector) =>
		rootSector.WorkspaceSector.Workspaces[
			rootSector.MapSector.MonitorWorkspaceMap[rootSector.MonitorSector.ActiveMonitorHandle]
		];

	/// <summary>
	/// Get the id of the active workspace.
	/// </summary>
	/// <returns>
	/// The id of the active workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<WorkspaceId> PickActiveWorkspaceId() =>
		static (IRootSector rootSector) =>
			rootSector.MapSector.MonitorWorkspaceMap[rootSector.MonitorSector.ActiveMonitorHandle];

	/// <summary>
	/// Get the active layout engine in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	/// <returns>
	/// The active layout engine in the provided workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<ILayoutEngine>> PickActiveLayoutEngine(WorkspaceId workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(workspaceId, rootSector, static workspace => workspace.GetActiveLayoutEngine());

	/// <summary>
	/// Get the active layout engine in the active workspace.
	/// </summary>
	/// <returns>
	/// The active layout engine in the active workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<ILayoutEngine> PickActiveLayoutEngine() =>
		(IRootSector rootSector) => PickActiveLayoutEngine(PickActiveWorkspaceId()(rootSector))(rootSector).Value;

	/// <summary>
	/// Get all the windows in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	/// <returns>
	/// All the windows in the provided workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IEnumerable<IWindow>>> PickWorkspaceWindows(WorkspaceId workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(workspaceId, rootSector, workspace => GetWorkspaceWindows(rootSector, workspace));

	/// <summary>
	/// Get all the windows in the active workspace.
	/// </summary>
	/// <returns>
	/// All the windows in the active workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<IEnumerable<IWindow>> PickActiveWorkspaceWindows() =>
		(IRootSector rootSector) => GetWorkspaceWindows(rootSector, PickActiveWorkspace()(rootSector));

	internal static IEnumerable<IWindow> GetWorkspaceWindows(IRootSector rootSector, IWorkspace workspace)
	{
		foreach (HWND hwnd in workspace.WindowPositions.Keys)
		{
			if (PickWindowByHandle(hwnd)(rootSector).TryGet(out IWindow window))
			{
				yield return window;
			}
		}
	}

	/// <summary>
	/// Get the last focused window in the provided workspace.
	/// </summary>
	/// <param name="workspaceId">The workspace to get the last focused window for. Defaults to the active workspace</param>
	/// <returns>
	/// The last focused window in the provided workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found or there is no last focused window, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<IWindow>> PickLastFocusedWindow(WorkspaceId workspaceId = default) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(
				workspaceId,
				rootSector,
				workspace =>
				{
					if (workspace.LastFocusedWindowHandle.IsNull)
					{
						return Result.FromError<IWindow>(new WhimError("No last focused window in workspace"));
					}

					return PickWindowByHandle(workspace.LastFocusedWindowHandle)(rootSector);
				}
			);

	/// <summary>
	/// Get the last focused window handle in the provided workspace.
	/// </summary>
	/// <param name="workspaceId">The workspace to get the last focused window handle for. Defaults to the active workspace</param>
	/// <returns>
	/// If the workspace is not found or there is no last focused window, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<HWND>> PickLastFocusedWindowHandle(WorkspaceId workspaceId = default) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(
				workspaceId,
				rootSector,
				workspace =>
				{
					if (workspace.LastFocusedWindowHandle.IsNull)
					{
						return Result.FromError<HWND>(new WhimError("No last focused window in workspace"));
					}

					return Result.FromValue(workspace.LastFocusedWindowHandle);
				}
			);

	/// <summary>
	/// Get the window position in the provided workspace.
	/// </summary>
	/// <param name="workspaceId">The workspace to get the window position for.</param>
	/// <param name="windowHandle">The window handle to get the position for.</param>
	/// <returns>
	/// The window position in the provided workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the workspace is not found or the window is not found in the workspace, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<WindowPosition>> PickWindowPosition(WorkspaceId workspaceId, HWND windowHandle) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(
				workspaceId,
				rootSector,
				workspace =>
				{
					if (workspace.WindowPositions.TryGetValue(windowHandle, out WindowPosition? position))
					{
						return position;
					}				return Result.FromError<WindowPosition>(
						StoreExceptions.WindowNotFoundInWorkspace(windowHandle, workspaceId)
					);
				}
			);

	/// <summary>
	/// Get the window position for the given <paramref name="windowHandle"/>.
	/// </summary>
	/// <param name="windowHandle">
	/// The window handle to get the position for.
	/// </param>
	/// <returns>
	/// The window position for the given <paramref name="windowHandle"/>, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// If the window is not found, then <see cref="Result{T, TError}.Error"/> will be returned.
	/// </returns>
	public static PurePicker<Result<WindowPosition>> PickWindowPosition(HWND windowHandle) =>
		(IRootSector rootSector) =>
		{
			Result<IWorkspace> workspaceResult = PickWorkspaceByWindow(windowHandle)(rootSector);
			if (workspaceResult.TryGet(out IWorkspace workspace))
			{
				return PickWindowPosition(workspace.Id, windowHandle)(rootSector);
			}

			return Result.FromError<WindowPosition>(workspaceResult.Error!);
		};

	/// <summary>
	/// Picks the function used to create the default layout engines to add to a workspace.
	/// </summary>
	/// <returns>
	/// The function used to create the default layout engines to add to a workspace, when passed to <see cref="IStore.Pick{TResult}(PurePicker{TResult})"/>.
	/// </returns>
	public static PurePicker<Func<CreateLeafLayoutEngine[]>> PickCreateLeafLayoutEngines() =>
		static (IRootSector rootSector) => rootSector.WorkspaceSector.CreateLayoutEngines;
}
