using System.Text.Json;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutPluginTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Name(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.tree_layout", name);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PluginCommands(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PreInitialize(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void PostInitialize(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	#region GetAddWindowDirection
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void GetAddWindowDirection_Monitor_NoLayoutEngine(IContext ctx, IMonitor monitor)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetAddWindowDirection_Monitor_IsNotTreeLayoutEngine(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		ILayoutEngine layoutEngine,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);
		workspace.ActiveLayoutEngine.Returns(layoutEngine);

		TreeLayoutPlugin plugin = new(ctx);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetAddWindowDirection_Monitor_LazyInit(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);

		TreeLayoutPlugin plugin = new(ctx);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(ctx, plugin, identity);
		workspace.ActiveLayoutEngine.Returns(treeLayoutEngine);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetAddWindowDirection_Monitor_AlreadyInit(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);

		TreeLayoutPlugin plugin = new(ctx);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(ctx, plugin, identity);
		workspace.ActiveLayoutEngine.Returns(treeLayoutEngine);

		plugin.SetAddWindowDirection(monitor, Direction.Left);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Left, direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void GetAddWindowDirection_Engine_LazyInit(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);
		TreeLayoutEngine engine = new(ctx, plugin, new LayoutEngineIdentity());

		// When
		Direction? direction = plugin.GetAddWindowDirection(engine);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void GetAddWindowDirection_Engine_AlreadyInit(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);
		TreeLayoutEngine engine = new(ctx, plugin, new LayoutEngineIdentity());

		plugin.SetAddWindowDirection(engine, Direction.Left);

		// When
		Direction? direction = plugin.GetAddWindowDirection(engine);

		// Then
		Assert.Equal(Direction.Left, direction);
	}
	#endregion

	#region SetAddWindowDirection
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetAddWindowDirection_NoLayoutEngine(IContext ctx, IMonitor monitor)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.SetAddWindowDirection(monitor, Direction.Up);
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetAddWindowDirection_NotTreeLayoutEngine(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		ILayoutEngine layoutEngine,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);

		TreeLayoutPlugin plugin = new(ctx);

		workspace.ActiveLayoutEngine.Returns(layoutEngine);

		// When
		plugin.SetAddWindowDirection(monitor, Direction.Up);
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetAddWindowDirection_DirectionNotSet(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);

		TreeLayoutPlugin plugin = new(ctx);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(ctx, plugin, identity);
		workspace.ActiveLayoutEngine.Returns(treeLayoutEngine);

		// When
		Assert.RaisedEvent<AddWindowDirectionChangedEventArgs> addWindowEvent =
			Assert.Raises<AddWindowDirectionChangedEventArgs>(
				h => plugin.AddWindowDirectionChanged += h,
				h => plugin.AddWindowDirectionChanged -= h,
				() => plugin.SetAddWindowDirection(monitor, Direction.Up)
			);

		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Up, addWindowEvent.Arguments.CurrentDirection);
		Assert.Equal(Direction.Right, addWindowEvent.Arguments.PreviousDirection);
		Assert.Equal(treeLayoutEngine, addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Up, direction);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetAddWindowDirection_DirectionAlreadySet(
		IContext ctx,
		IMonitor monitor,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);

		TreeLayoutPlugin plugin = new(ctx);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(ctx, plugin, identity);
		workspace.ActiveLayoutEngine.Returns(treeLayoutEngine);

		// When
		plugin.SetAddWindowDirection(monitor, Direction.Up);
		Assert.RaisedEvent<AddWindowDirectionChangedEventArgs> addWindowEvent =
			Assert.Raises<AddWindowDirectionChangedEventArgs>(
				h => plugin.AddWindowDirectionChanged += h,
				h => plugin.AddWindowDirectionChanged -= h,
				() => plugin.SetAddWindowDirection(monitor, Direction.Down)
			);

		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Down, addWindowEvent.Arguments.CurrentDirection);
		Assert.Equal(Direction.Up, addWindowEvent.Arguments.PreviousDirection);
		Assert.Equal(treeLayoutEngine, addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Down, direction);
	}
	#endregion

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void LoadState(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void SaveState(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
