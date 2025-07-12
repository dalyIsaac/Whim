using System.Linq;
using FluentAssertions;

namespace Whim.Tests;

public class InitializeWorkspacesTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		InitializeWorkspacesTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowAddedEventArgs>(
			h => rootSector.WindowSector.WindowAdded += h,
			h => rootSector.WindowSector.WindowAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static (Result<Unit>, List<WindowAddedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector rootSector,
		InitializeWorkspacesTransform sut
	)
	{
		Result<Unit>? result = null;
		List<WindowAddedEventArgs> evs = new();
		CustomAssert.Raises<WindowAddedEventArgs>(
			h => rootSector.WindowSector.WindowAdded += h,
			h => rootSector.WindowSector.WindowAdded -= h,
			() => result = ctx.Store.Dispatch(sut),
			(sender, args) => evs.Add(args)
		);
		return (result!.Value, evs);
	}

	private static void AddWorkspacesToSavedState(IInternalContext internalCtx, params SavedWorkspace[] workspaces)
	{
		internalCtx.CoreSavedStateManager.SavedState.Returns(new CoreSavedState([.. workspaces]));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoSavedWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		// Given there are no saved workspaces
		InitializeWorkspacesTransform sut = new();

		// When
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CouldNotFindWorkspace(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there are saved workspaces which don't exist in the workspace manager
		SavedWorkspace workspace = new("test", new List<SavedWindow>(), null);
		AddWorkspacesToSavedState(internalCtx, workspace);

		InitializeWorkspacesTransform sut = new();

		// When the map transform is dispatched
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CouldNotFindWindowFromHandle(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there are saved workspaces which don't exist in the workspace manager
		IWindow window = CreateWindow((HWND)10);
		SavedWorkspace workspace = new("test", [new SavedWindow(window.Handle, Rectangle.UnitSquare<double>())], null);
		AddWorkspacesToSavedState(internalCtx, workspace);

		ctx.CreateWindow(window.Handle).Returns(Result.FromException<IWindow>(new Exception("nope")));

		InitializeWorkspacesTransform sut = new();

		// When the map transform is dispatched
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
	}

	private static HWND BrowserHandle => (HWND)1;
	private static HWND SpotifyHandle => (HWND)2;
	private static HWND BrokenHandle => (HWND)3;
	private static HWND DiscordHandle => (HWND)4;
	private static HWND VscodeHandle => (HWND)5;

	private static string BrowserWorkspaceName => "Browser";
	private static string MediaWorkspaceName => "Media";
	private static string CodeWorkspaceName => "Code";
	private static string StickyWorkspaceName => "Sticky";
	private static string SavedStickyWorkspaceName => "SavedSticky";

	private static HMONITOR BrowserMonitor => (HMONITOR)1;
	private static HMONITOR CodeMonitor => (HMONITOR)2;
	private static HMONITOR AutoMonitor => (HMONITOR)3;

	private static void Setup_UserCreatedWorkspaces(MutableRootSector root)
	{
		root.WorkspaceSector.WorkspacesToCreate =
		[
			new WorkspaceToCreate(Guid.NewGuid(), BrowserWorkspaceName, null, null),
			new WorkspaceToCreate(
				Guid.NewGuid(),
				CodeWorkspaceName,
				[(id) => new ImmutableTestLayoutEngine(), (id) => new ImmutableTestLayoutEngine()],
				null
			),
			new WorkspaceToCreate(Guid.NewGuid(), StickyWorkspaceName, null, [0, 1]),
		];
	}

	private static void Setup_SavedState(IInternalContext internalCtx)
	{
		SavedWindow browserWindow = new(BrowserHandle, Rectangle.UnitSquare<double>());
		SavedWindow spotifyWindow = new(SpotifyHandle, Rectangle.UnitSquare<double>());
		SavedWindow brokenWindow = new(BrokenHandle, Rectangle.UnitSquare<double>());
		SavedWindow discordWindow = new(DiscordHandle, Rectangle.UnitSquare<double>());

		SavedWorkspace browserWorkspace = new(BrowserWorkspaceName, [browserWindow, brokenWindow], null);
		SavedWorkspace mediaWorkspace = new(MediaWorkspaceName, [spotifyWindow, discordWindow], null);
		SavedWorkspace savedStickyWorkspace = new(SavedStickyWorkspaceName, [], [1, 2]);

		AddWorkspacesToSavedState(internalCtx, browserWorkspace, mediaWorkspace, savedStickyWorkspace);
	}

	private static void Setup_CreateWindow(IContext ctx)
	{
		IWindow browserWindow = CreateWindow(BrowserHandle);
		IWindow discordWindow = CreateWindow(DiscordHandle);
		IWindow spotifyWindow = CreateWindow(SpotifyHandle);
		IWindow vscodeWindow = CreateWindow(VscodeHandle);

		ctx.CreateWindow(BrowserHandle).Returns(Result.FromValue(browserWindow));
		ctx.CreateWindow(DiscordHandle).Returns(Result.FromValue(discordWindow));
		ctx.CreateWindow(SpotifyHandle).Returns(Result.FromValue(spotifyWindow));
		ctx.CreateWindow(BrokenHandle).Returns(Result.FromException<IWindow>(new Exception("nope")));
		ctx.CreateWindow(VscodeHandle).Returns(Result.FromValue(vscodeWindow));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PopulateSavedWorkspaces(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given:
		// - the user has created the "Browser", "Code", and "Sticky" workspaces
		Setup_UserCreatedWorkspaces(rootSector);

		// - the user has saved the "Browser", "Media", and "SavedSticky" workspaces
		//   - "Browser" has a browser window, a Spotify window, and a broken window
		//   - "Media" has a saved Discord window
		//   - "SavedSticky" has no windows
		Setup_SavedState(internalCtx);

		// - the Broken window fails to create
		// - there's a new vscode window
		Setup_CreateWindow(ctx);

		// - the Spotify and Discord window will appear in the newly created workspace's monitor
		internalCtx
			.CoreNativeManager.MonitorFromWindow(
				Arg.Is<HWND>(h => h == SpotifyHandle || h == DiscordHandle),
				MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
			)
			.Returns(_ => AutoMonitor);

		// - the vscode window will appear in the "Code" workspace's monitor
		internalCtx
			.CoreNativeManager.MonitorFromWindow(
				Arg.Is<HWND>(h => h == VscodeHandle),
				MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
			)
			.Returns(_ => CodeMonitor);

		// - there are three monitors
		AddMonitorsToManager(
			ctx,
			rootSector,
			CreateMonitor(BrowserMonitor),
			CreateMonitor(CodeMonitor),
			CreateMonitor(AutoMonitor)
		);

		internalCtx
			.CoreNativeManager.GetAllWindows()
			.Returns(_ => new List<HWND>() { BrowserHandle, DiscordHandle, SpotifyHandle, BrokenHandle, VscodeHandle });

		internalCtx.CoreNativeManager.IsStandardWindow(Arg.Any<HWND>()).Returns(true);
		internalCtx.CoreNativeManager.HasNoVisibleOwner(Arg.Any<HWND>()).Returns(true);

		rootSector.WorkspaceSector.CreateLayoutEngines = () => [id => new ImmutableTestLayoutEngine()];

		InitializeWorkspacesTransform sut = new();

		// When the map transform is dispatched
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then:
		Assert.True(result.IsSuccessful);

		// - 4 windows have been added
		Assert.Equal(4, evs.Count);
		Assert.Equal(4, rootSector.WindowSector.Windows.Count);
		Assert.Equal(4, rootSector.MapSector.WindowWorkspaceMap.Count);

		// - there are 4 workspaces
		Assert.Equal(4, rootSector.WorkspaceSector.Workspaces.Count);

		// - the "Browser" workspace has been added with the "Browser", "Spotify", and "Discord"  windows
		Workspace browserWorkspace = rootSector.WorkspaceSector.Workspaces.Values.FirstOrDefault(w =>
			w.Name == BrowserWorkspaceName
		)!;
		Assert.Single(browserWorkspace.WindowPositions);
		Assert.Contains(BrowserHandle, browserWorkspace.WindowPositions);

		Assert.Single(browserWorkspace.LayoutEngines);

		// - the new "Code" workspace has been added with the new "vscode" window
		Workspace codeWorkspace = rootSector.WorkspaceSector.Workspaces.Values.FirstOrDefault(w =>
			w.Name == CodeWorkspaceName
		)!;
		Assert.Single(codeWorkspace.WindowPositions);
		Assert.Contains(VscodeHandle, codeWorkspace.WindowPositions);

		Assert.Equal(2, codeWorkspace.LayoutEngines.Count);

		// - the "Sticky" workspace has been added with no windows
		Workspace stickyWorkspace = rootSector.WorkspaceSector.Workspaces.Values.FirstOrDefault(w =>
			w.Name == StickyWorkspaceName
		)!;
		Assert.Empty(stickyWorkspace.WindowPositions);

		Assert.Single(stickyWorkspace.LayoutEngines);
		Assert.Single(rootSector.MapSector.StickyWorkspaceMonitorIndexMap);
		rootSector.MapSector.StickyWorkspaceMonitorIndexMap[stickyWorkspace.Id].Should().BeEquivalentTo(new[] { 0, 1 });

		// - the automatically created workspace has the "Spotify" and "Discord" windows
		Workspace autoWorkspace = rootSector.WorkspaceSector.Workspaces.Values.FirstOrDefault(w =>
			w.Name == "Workspace 4"
		)!;
		Assert.Equal(2, autoWorkspace.WindowPositions.Count);
		Assert.Contains(SpotifyHandle, autoWorkspace.WindowPositions);
		Assert.Contains(DiscordHandle, autoWorkspace.WindowPositions);

		Assert.Single(autoWorkspace.LayoutEngines);

		// - the WorkspaceSector has initialized
		Assert.True(rootSector.WorkspaceSector.HasInitialized);

		// - the workspaces are activated on each of the three monitors
		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);
		Assert.Equal(browserWorkspace.Id, rootSector.MapSector.MonitorWorkspaceMap[BrowserMonitor]);
		Assert.Equal(codeWorkspace.Id, rootSector.MapSector.MonitorWorkspaceMap[CodeMonitor]);
		Assert.Equal(autoWorkspace.Id, rootSector.MapSector.MonitorWorkspaceMap[AutoMonitor]);

		// - the startup windows are set
		Assert.Equal(5, rootSector.WindowSector.StartupWindows.Count);
		rootSector
			.WindowSector.StartupWindows.Should()
			.BeEquivalentTo(new[] { BrowserHandle, DiscordHandle, SpotifyHandle, BrokenHandle, VscodeHandle });
	}
}
