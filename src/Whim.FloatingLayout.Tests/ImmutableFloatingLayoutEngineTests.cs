using Moq;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class ImmutableFloatingLayoutEngineTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();

		public Mock<IFloatingLayoutPlugin> FloatingLayoutPlugin { get; } = new();
		public Mock<IInternalFloatingLayoutPlugin>? InternalFloatingLayoutPlugin { get; private set; }

		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		}

		public Wrapper Setup_InternalFloatingLayoutPlugin()
		{
			InternalFloatingLayoutPlugin = FloatingLayoutPlugin.As<IInternalFloatingLayoutPlugin>();
			InternalFloatingLayoutPlugin.Setup(iflp => iflp.MutableFloatingWindows).Returns(new HashSet<IWindow>());

			return this;
		}

		public Wrapper Setup_FloatingLayoutPlugin_False()
		{
			FloatingLayoutPlugin.Setup(flp => flp.FloatingWindows.Contains(It.IsAny<IWindow>())).Returns(false);
			return this;
		}

		public Wrapper Setup_FloatingLayoutPlugin_True(IWindow window)
		{
			FloatingLayoutPlugin.Setup(flp => flp.FloatingWindows.Contains(window)).Returns(true);
			InternalFloatingLayoutPlugin?.Setup(iflp => iflp.MutableFloatingWindows.Contains(window)).Returns(true);
			return this;
		}

		public Wrapper Setup_DwmGetWindowLocation_Null()
		{
			NativeManager.Setup(x => x.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((Location<int>?)null);

			return this;
		}

		public Wrapper Setup_DwmGetWindowLocation_NotNull(Location<int> location)
		{
			NativeManager.Setup(x => x.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(location);

			return this;
		}

		public Wrapper Setup_GetMonitorAtPoint()
		{
			Mock<IMonitor> monitor = new();
			monitor
				.Setup(m => m.WorkingArea)
				.Returns(
					new Location<int>()
					{
						X = 100,
						Y = 100,
						Width = 100,
						Height = 100
					}
				);

			MonitorManager.Setup(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(monitor.Object);

			return this;
		}

		public Wrapper Setup_GetWorkspaceForMonitor(IWorkspace workspace)
		{
			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(It.IsAny<IMonitor>())).Returns(workspace);

			return this;
		}
	}

	#region Add
	[Fact]
	public void Add_UseInner()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Setup_FloatingLayoutPlugin_False();

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Once);
	}

	[Fact]
	public void Add_AlreadyTrackedByPlugin_NoActualLocation()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper.Setup_FloatingLayoutPlugin_True(window.Object);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.Same(engine, result);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Never);
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByPlugin_IsNotInternalFloatingLayoutPlugin()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_DwmGetWindowLocation_NotNull(new Location<int>())
			.Setup_GetMonitorAtPoint()
			.Setup_GetWorkspaceForMonitor(workspace.Object);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.Same(engine, result);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByPlugin_IsInternalFloatingLayoutPlugin_NotWorkspace()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_DwmGetWindowLocation_NotNull(new Location<int>())
			.Setup_GetMonitorAtPoint()
			.Setup_GetWorkspaceForMonitor(null!);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.Same(engine, result);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByPlugin_Success()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_DwmGetWindowLocation_NotNull(new Location<int>())
			.Setup_GetMonitorAtPoint()
			.Setup_GetWorkspaceForMonitor(workspace.Object);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByLayoutEngine_SameLocation()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 1,
					Y = 1,
					Width = 1,
					Height = 1
				}
			)
			.Setup_GetMonitorAtPoint()
			.Setup_GetWorkspaceForMonitor(workspace.Object);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);
		ILayoutEngine result2 = result.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.Same(result, result2);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Exactly(2));
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Exactly(2));
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_GetWorkspaceForMonitor(workspace.Object)
			.Setup_GetMonitorAtPoint()
			.Setup_InternalFloatingLayoutPlugin();

		wrapper.NativeManager
			.SetupSequence(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()))
			.Returns(
				new Location<int>()
				{
					X = 1,
					Y = 1,
					Width = 1,
					Height = 1
				}
			)
			.Returns(
				new Location<int>()
				{
					X = 2,
					Y = 2,
					Width = 2,
					Height = 2
				}
			);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);
		ILayoutEngine result2 = result.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.NotSame(result, result2);
		workspace.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Exactly(2));
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Exactly(2));
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Exactly(2));
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void AddAtPoint()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Setup_FloatingLayoutPlugin_False();

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.AddAtPoint(window.Object, new Point<double>());

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Once);
	}
	#endregion

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();
		Mock<IWindow> window2 = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetMonitorAtPoint()
			.Setup_GetWorkspaceForMonitor(workspace.Object);

		ImmutableFloatingLayoutEngine engine =
			new(
				wrapper.Context.Object,
				wrapper.FloatingLayoutPlugin.Object,
				new ImmutableColumnLayoutEngine(new LayoutEngineIdentity())
			);

		// When
		ILayoutEngine result = engine.Add(window.Object);
		ILayoutEngine result2 = result.Add(window2.Object);
		IWindowState[] states = result2
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, new Mock<IMonitor>().Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.NotSame(result, result2);
		Assert.Equal(2, states.Length);

		Assert.Equal(
			new Location<int>()
			{
				X = 50,
				Y = 50,
				Width = 50,
				Height = 50
			},
			states[0].Location
		);
		Assert.Equal(new Location<int>() { Width = 100, Height = 100 }, states[1].Location);
	}

	#region Remove
	[Fact]
	public void Remove_TrackedByLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_InternalFloatingLayoutPlugin()
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetWorkspaceForMonitor(workspace.Object)
			.Setup_GetMonitorAtPoint()
			.Setup_InternalFloatingLayoutPlugin();

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Add(window.Object);
		ILayoutEngine result2 = result.Remove(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.NotSame(result, result2);
		wrapper.InnerLayoutEngine.Verify(x => x.Remove(window.Object), Times.Never);
	}

	[Fact]
	public void Remove_Inner()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.Remove(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.InnerLayoutEngine.Verify(x => x.Remove(window.Object), Times.Once);
	}
	#endregion

	[Fact]
	public void SwapWindowInDirection()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper.InnerLayoutEngine
			.Setup(x => x.SwapWindowInDirection(Direction.Left, window.Object))
			.Returns(wrapper.InnerLayoutEngine.Object);

		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window.Object);

		// Then
		Assert.Same(engine, result);
	}
}
