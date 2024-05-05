using System.Text.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutPluginTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Name(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.slice_layout", name);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PluginCommands(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PreInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PostInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LoadState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.LoadState(default);

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SaveState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	#region PromoteWindowInStack
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		bool result = plugin.PromoteWindowInStack();

		// Then nothing
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteWindowInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		// When
		bool result = plugin.PromoteWindowInStack(window);

		// Then nothing
		IWorkspace activeWorkspace = ctx.WorkspaceManager.ActiveWorkspace;
		activeWorkspace.DidNotReceive().PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
		activeWorkspace.DidNotReceive().DoLayout();
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteWindowInStack(
		IContext ctx,
		IWindow window,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		// When
		bool result = plugin.PromoteWindowInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(action =>
					action.Name == plugin.PromoteWindowActionName && action.Window == window
				)
			);
	}
	#endregion

	#region DemoteWindowInStack
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		bool result = plugin.DemoteWindowInStack();

		// Then nothing
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteWindowInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		// When
		bool result = plugin.DemoteWindowInStack(window);

		// Then nothing
		IWorkspace activeWorkspace = ctx.WorkspaceManager.ActiveWorkspace;
		activeWorkspace.DidNotReceive().PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
		activeWorkspace.DidNotReceive().DoLayout();
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteWindowInStack(
		IContext ctx,
		IWindow window,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		// When
		bool result = plugin.DemoteWindowInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(action =>
					action.Name == plugin.DemoteWindowActionName && action.Window == window
				)
			);
		Assert.True(result);
	}
	#endregion

	[Theory]
	[InlineAutoSubstituteData(WindowInsertionType.Swap)]
	[InlineAutoSubstituteData(WindowInsertionType.Rotate)]
	public void WindowInsertionType_Set(WindowInsertionType insertionType)
	{
		// Given
		SliceLayoutPlugin plugin =
			new(Substitute.For<IContext>())
			{
				// When
				WindowInsertionType = insertionType
			};

		// Then
		Assert.Equal(insertionType, plugin.WindowInsertionType);
	}

	#region PromoteFocusInStack
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		bool result = plugin.PromoteFocusInStack();

		// Then nothing
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteFocusInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		// When
		bool result = plugin.PromoteFocusInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PromoteFocusInStack(
		IContext ctx,
		IWindow window,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		// When
		bool result = plugin.PromoteFocusInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(action =>
					action.Name == plugin.PromoteFocusActionName && action.Window == window
				)
			);
		Assert.True(result);
	}
	#endregion

	#region DemoteFocusInStack
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		bool result = plugin.DemoteFocusInStack();

		// Then nothing
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteFocusInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		// When
		bool result = plugin.DemoteFocusInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DemoteFocusInStack(
		IContext ctx,
		IWindow window,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		// When
		bool result = plugin.DemoteFocusInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(action =>
					action.Name == plugin.DemoteFocusActionName && action.Window == window
				)
			);
		Assert.True(result);
	}
	#endregion
}
