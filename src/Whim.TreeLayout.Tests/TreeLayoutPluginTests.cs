using Moq;
using System.Text.Json;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWindowManager> WindowManager { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.WindowManager).Returns(WindowManager.Object);
		}

		public Wrapper Setup_GetWorkspaceForMonitor(Mock<IMonitor> monitor, IWorkspace workspace)
		{
			WorkspaceManager.Setup(wm => wm.GetWorkspaceForMonitor(monitor.Object)).Returns(workspace);
			return this;
		}
	}

	[Fact]
	public void Name()
	{
		// Given
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.tree_layout", name);
	}

	[Fact]
	public void PluginCommands()
	{
		// Given
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		IPluginCommands commands = plugin.PluginCommands;

		// Then
		Assert.NotEmpty(commands.Commands);
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.PreInitialize();

		// Then nothing
		wrapper.Context.VerifyGet(c => c.WorkspaceManager, Times.Never);
		wrapper.Context.VerifyGet(c => c.WindowManager, Times.Never);
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.PostInitialize();

		// Then nothing
		wrapper.Context.VerifyGet(c => c.WorkspaceManager, Times.Never);
		wrapper.Context.VerifyGet(c => c.WindowManager, Times.Never);
	}

	#region GetAddWindowDirection
	[Fact]
	public void GetAddWindowDirection_Monitor_NoLayoutEngine()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Null(direction);
	}

	[Fact]
	public void GetAddWindowDirection_Monitor_IsNotTreeLayoutEngine()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		workspace.Setup(w => w.ActiveLayoutEngine).Returns(layoutEngine.Object);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Null(direction);
	}

	[Fact]
	public void GetAddWindowDirection_Monitor_LazyInit()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(wrapper.Context.Object, plugin, identity);
		workspace.Setup(w => w.ActiveLayoutEngine).Returns(treeLayoutEngine);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Fact]
	public void GetAddWindowDirection_Monitor_AlreadyInit()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(wrapper.Context.Object, plugin, identity);
		workspace.Setup(w => w.ActiveLayoutEngine).Returns(treeLayoutEngine);

		plugin.SetAddWindowDirection(monitor.Object, Direction.Left);

		// When
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Equal(Direction.Left, direction);
	}

	[Fact]
	public void GetAddWindowDirection_Engine_LazyInit()
	{
		// Given
		Mock<IContext> context = new();
		TreeLayoutPlugin plugin = new(context.Object);
		TreeLayoutEngine engine = new(context.Object, plugin, new LayoutEngineIdentity());

		// When
		Direction? direction = plugin.GetAddWindowDirection(engine);

		// Then
		Assert.Equal(Direction.Right, direction);
	}

	[Fact]
	public void GetAddWindowDirection_Engine_AlreadyInit()
	{
		// Given
		Mock<IContext> context = new();
		TreeLayoutPlugin plugin = new(context.Object);
		TreeLayoutEngine engine = new(context.Object, plugin, new LayoutEngineIdentity());

		plugin.SetAddWindowDirection(engine, Direction.Left);

		// When
		Direction? direction = plugin.GetAddWindowDirection(engine);

		// Then
		Assert.Equal(Direction.Left, direction);
	}
	#endregion

	#region SetAddWindowDirection
	[Fact]
	public void SetAddWindowDirection_NoLayoutEngine()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Wrapper wrapper = new();
		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.SetAddWindowDirection(monitor.Object, Direction.Up);
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Null(direction);
	}

	[Fact]
	public void SetAddWindowDirection_NotTreeLayoutEngine()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		workspace.Setup(w => w.ActiveLayoutEngine).Returns(layoutEngine.Object);

		// When
		plugin.SetAddWindowDirection(monitor.Object, Direction.Up);
		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Null(direction);
	}

	[Fact]
	public void SetAddWindowDirection_DirectionNotSet()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(wrapper.Context.Object, plugin, identity);
		workspace.Setup(w => w.ActiveLayoutEngine).Returns(treeLayoutEngine);

		// When
		Assert.RaisedEvent<AddWindowDirectionChangedEventArgs> addWindowEvent =
			Assert.Raises<AddWindowDirectionChangedEventArgs>(
				h => plugin.AddWindowDirectionChanged += h,
				h => plugin.AddWindowDirectionChanged -= h,
				() => plugin.SetAddWindowDirection(monitor.Object, Direction.Up)
			);

		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Equal(Direction.Up, addWindowEvent.Arguments.CurrentDirection);
		Assert.Equal(Direction.Right, addWindowEvent.Arguments.PreviousDirection);
		Assert.Equal(treeLayoutEngine, addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Up, direction);
	}

	[Fact]
	public void SetAddWindowDirection_DirectionAlreadySet()
	{
		// Given
		Mock<IMonitor> monitor = new();
		Mock<IWorkspace> workspace = new();
		Wrapper wrapper = new Wrapper().Setup_GetWorkspaceForMonitor(monitor, workspace.Object);

		TreeLayoutPlugin plugin = new(wrapper.Context.Object);

		LayoutEngineIdentity identity = new();
		TreeLayoutEngine treeLayoutEngine = new(wrapper.Context.Object, plugin, identity);
		workspace.Setup(w => w.ActiveLayoutEngine).Returns(treeLayoutEngine);

		// When
		plugin.SetAddWindowDirection(monitor.Object, Direction.Up);
		Assert.RaisedEvent<AddWindowDirectionChangedEventArgs> addWindowEvent =
			Assert.Raises<AddWindowDirectionChangedEventArgs>(
				h => plugin.AddWindowDirectionChanged += h,
				h => plugin.AddWindowDirectionChanged -= h,
				() => plugin.SetAddWindowDirection(monitor.Object, Direction.Down)
			);

		Direction? direction = plugin.GetAddWindowDirection(monitor.Object);

		// Then
		Assert.Equal(Direction.Down, addWindowEvent.Arguments.CurrentDirection);
		Assert.Equal(Direction.Up, addWindowEvent.Arguments.PreviousDirection);
		Assert.Equal(treeLayoutEngine, addWindowEvent.Arguments.TreeLayoutEngine);
		Assert.Equal(Direction.Down, direction);
	}
	#endregion

	[Fact]
	public void LoadState()
	{
		// Given
		Mock<IContext> context = new();
		TreeLayoutPlugin plugin = new(context.Object);

		// When
		plugin.LoadState(JsonDocument.Parse("{}").RootElement);

		// Then nothing
		context.Verify(c => c.WorkspaceManager, Times.Never);
		context.Verify(c => c.WindowManager, Times.Never);
	}

	[Fact]
	public void SaveState()
	{
		// Given
		Mock<IContext> context = new();
		TreeLayoutPlugin plugin = new(context.Object);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
