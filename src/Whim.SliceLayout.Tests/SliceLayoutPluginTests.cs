using System.Text.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutPluginTests
{
	[Theory, AutoSubstituteData]
	public void Name(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.slice_layout", name);
	}

	[Theory, AutoSubstituteData]
	public void PluginCommands(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData]
	public void PreInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData]
	public void PostInitialize(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData]
	public void LoadState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		plugin.LoadState(default);

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData]
	public void SaveState(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	#region PromoteWindowInStack
	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		plugin.PromoteWindowInStack();

		// Then nothing
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).ReturnsNull();

		// When
		plugin.PromoteWindowInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
	}

	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).Returns(workspace);

		// When
		plugin.PromoteWindowInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(
					action => action.Name == plugin.PromoteWindowActionName && action.Window == window
				)
			);
	}
	#endregion

	#region DemoteWindowInStack
	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		plugin.DemoteWindowInStack();

		// Then nothing
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).ReturnsNull();

		// When
		plugin.DemoteWindowInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
	}

	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).Returns(workspace);

		// When
		plugin.DemoteWindowInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(
					action => action.Name == plugin.DemoteWindowActionName && action.Window == window
				)
			);
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
	[Theory, AutoSubstituteData]
	public void PromoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		plugin.PromoteFocusInStack();

		// Then nothing
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData]
	public void PromoteFocusInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).ReturnsNull();

		// When
		plugin.PromoteFocusInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
	}

	[Theory, AutoSubstituteData]
	public void PromoteFocusInStack(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).Returns(workspace);

		// When
		plugin.PromoteFocusInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(
					action => action.Name == plugin.PromoteFocusActionName && action.Window == window
				)
			);
	}
	#endregion

	#region DemoteFocusInStack
	[Theory, AutoSubstituteData]
	public void DemoteFocusInStack_NoWindow(IContext ctx)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.ReturnsNull();

		// When
		plugin.DemoteFocusInStack();

		// Then nothing
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData]
	public void DemoteFocusInStack_NoWorkspace(IContext ctx, IWindow window)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).ReturnsNull();

		// When
		plugin.DemoteFocusInStack(window);

		// Then nothing
		ctx.WorkspaceManager.ActiveWorkspace.DidNotReceive()
			.PerformCustomLayoutEngineAction(Arg.Any<LayoutEngineCustomAction>());
	}

	[Theory, AutoSubstituteData]
	public void DemoteFocusInStack(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		SliceLayoutPlugin plugin = new(ctx);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);
		ctx.WorkspaceManager.GetWorkspaceForWindow(window).Returns(workspace);

		// When
		plugin.DemoteFocusInStack(window);

		// Then
		workspace
			.Received(1)
			.PerformCustomLayoutEngineAction(
				Arg.Is<LayoutEngineCustomAction>(
					action => action.Name == plugin.DemoteFocusActionName && action.Window == window
				)
			);
	}
	#endregion
}
