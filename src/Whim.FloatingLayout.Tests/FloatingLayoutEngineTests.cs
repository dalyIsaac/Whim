using Moq;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutEngineTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();

		public Mock<IFloatingLayoutPlugin> FloatingLayoutPlugin { get; }
		public Mock<IInternalFloatingLayoutPlugin>? InternalFloatingLayoutPlugin { get; }

		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();

		public Wrapper(bool setupInternalPlugin = false)
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			if (setupInternalPlugin)
			{
				InternalFloatingLayoutPlugin = new();
				FloatingLayoutPlugin = InternalFloatingLayoutPlugin.As<IFloatingLayoutPlugin>();
				InternalFloatingLayoutPlugin.Setup(iflp => iflp.MutableFloatingWindows).Returns(new HashSet<IWindow>());
			}
			else
			{
				FloatingLayoutPlugin = new();
			}
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
	}

	#region Add
	[Fact]
	public void Add_UseInner()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Setup_FloatingLayoutPlugin_False();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Add_AlreadyTrackedByPlugin_NoActualLocation()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper.Setup_FloatingLayoutPlugin_True(window.Object);

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.AddWindow(window.Object);

		// Then
		Assert.Same(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByLayoutEngine_LocationHasNotChanged()
	{
		// Given
		Wrapper wrapper = new(setupInternalPlugin: true);

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetMonitorAtPoint();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.AddWindow(window.Object);
		ILayoutEngine result2 = result.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.Same(result, result2);

		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Exactly(2));
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Exactly(2));
	}

	[Fact]
	public void AddAtPoint()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Setup_FloatingLayoutPlugin_False();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.AddAtPoint(window.Object, new Point<double>());

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindow(window.Object), Times.Once);
	}
	#endregion

	#region Remove
	[Fact]
	public void Remove_UseInner()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Setup_FloatingLayoutPlugin_False();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Remove_TrackedByLayoutEngine_PluginIsNotInternalPlugin()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetMonitorAtPoint();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.AddWindow(window.Object).RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		wrapper.InternalFloatingLayoutPlugin?.Verify(
			iflp => iflp.MutableFloatingWindows.Remove(window.Object),
			Times.Never
		);
	}

	[Fact]
	public void Remove_TrackedByLayoutEngine()
	{
		// Given
		Wrapper wrapper = new(setupInternalPlugin: true);

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetMonitorAtPoint();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.AddWindow(window.Object).RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.RemoveWindow(window.Object), Times.Never);
		wrapper.InternalFloatingLayoutPlugin?.Verify(
			iflp => iflp.MutableFloatingWindows.Remove(window.Object),
			Times.Once
		);
	}

	#endregion

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new(setupInternalPlugin: true);

		Mock<IWindow> window = new();
		Mock<IWindow> window2 = new();

		wrapper
			.Setup_FloatingLayoutPlugin_True(window.Object)
			.Setup_DwmGetWindowLocation_NotNull(
				new Location<int>()
				{
					X = 50,
					Y = 50,
					Width = 50,
					Height = 50
				}
			)
			.Setup_GetMonitorAtPoint();

		FloatingLayoutEngine engine =
			new(
				wrapper.Context.Object,
				wrapper.FloatingLayoutPlugin.Object,
				new ColumnLayoutEngine(new LayoutEngineIdentity())
			);

		// When
		ILayoutEngine result = engine.AddWindow(window.Object);
		ILayoutEngine result2 = result.AddWindow(window2.Object);
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

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window.Object);

		// Then
		Assert.Same(engine, result);
	}
}
