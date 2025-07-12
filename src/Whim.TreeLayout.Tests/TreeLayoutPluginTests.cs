using System.Text.Json;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutPluginTests
{
	private class Customization : StoreCustomization
	{
		public static Workspace CreateWorkspace()
		{
			LayoutEngineIdentity identity = new();
			ILayoutEngine layoutEngine = Substitute.For<ILayoutEngine>();
			layoutEngine.Identity.Returns(identity);

			Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine] };
			return workspace;
		}

		public static Workspace CreateTreeWorkspace(IContext ctx, TreeLayoutPlugin plugin)
		{
			LayoutEngineIdentity identity = new();
			TreeLayoutEngine layoutEngine = new(ctx, plugin, identity);
			return CreateWorkspace() with { LayoutEngines = [layoutEngine] };
		}

		public static IMonitor CreateMonitor(IContext ctx)
		{
			IMonitor monitor = Substitute.For<IMonitor>();
			monitor.Handle.Returns((HMONITOR)1);
			return monitor;
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Name(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.tree_layout", name);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PluginCommands(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PreInitialize(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void PostInitialize(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	#region GetAddWindowDirection
	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Monitor_NoLayoutEngine(IContext ctx, IMonitor monitor)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Monitor_IsNotTreeLayoutEngine(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		IMonitor monitor = Customization.CreateMonitor(ctx);
		PopulateMonitorWorkspaceMap(root, monitor, StoreTestUtils.CreateWorkspace());

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Monitor_LazyInit(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		IMonitor monitor = Customization.CreateMonitor(ctx);
		PopulateMonitorWorkspaceMap(root, monitor, Customization.CreateTreeWorkspace(ctx, plugin));

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Monitor_AlreadyInit(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		IMonitor monitor = Customization.CreateMonitor(ctx);
		PopulateMonitorWorkspaceMap(root, monitor, Customization.CreateTreeWorkspace(ctx, plugin));

		plugin.SetAddWindowDirection(monitor, Direction.Left);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Equal(Direction.Left, direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Engine_LazyInit(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);
		TreeLayoutEngine engine = new(ctx, plugin, new LayoutEngineIdentity());

		// When
		Direction? direction = plugin.GetAddWindowDirection(engine);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void GetAddWindowDirection_Engine_AlreadyInit(IContext ctx)
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
	[Theory, AutoSubstituteData<Customization>]
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

	[Theory, AutoSubstituteData<Customization>]
	internal void SetAddWindowDirection_NotTreeLayoutEngine(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		IMonitor monitor = Customization.CreateMonitor(ctx);
		PopulateMonitorWorkspaceMap(root, monitor, Customization.CreateWorkspace());

		// When
		plugin.SetAddWindowDirection(monitor, Direction.Up);
		Direction? direction = plugin.GetAddWindowDirection(monitor);

		// Then
		Assert.Null(direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void SetAddWindowDirection_DirectionNotSet(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		NativeManagerUtils.SetupTryEnqueue(ctx);
		IMonitor monitor = Customization.CreateMonitor(ctx);
		Workspace workspace = Customization.CreateTreeWorkspace(ctx, plugin);
		PopulateMonitorWorkspaceMap(root, monitor, workspace);

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
		Assert.Equal(workspace.LayoutEngines[0], addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Up, direction);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void SetAddWindowDirection_DirectionAlreadySet(IContext ctx, MutableRootSector root)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		NativeManagerUtils.SetupTryEnqueue(ctx);
		IMonitor monitor = Customization.CreateMonitor(ctx);
		Workspace workspace = Customization.CreateTreeWorkspace(ctx, plugin);
		PopulateMonitorWorkspaceMap(root, monitor, workspace);

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
		Assert.Equal(workspace.LayoutEngines[0], addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Down, direction);
	}
	#endregion

	[Theory, AutoSubstituteData<Customization>]
	internal void LoadState(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		CustomAssert.NoContextCalls(ctx);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void SaveState(IContext ctx)
	{
		// Given
		TreeLayoutPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
