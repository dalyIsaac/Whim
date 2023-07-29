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

		public FloatingLayoutPlugin FloatingLayoutPlugin { get; }

		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			FloatingLayoutPlugin = new FloatingLayoutPlugin(Context.Object);
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

		public Wrapper Setup_PluginTracksLocation(
			Mock<IWorkspace> workspace,
			Mock<IWindow> window,
			ILayoutEngine engine
		)
		{
			WorkspaceManager.Setup(x => x.GetWorkspaceForWindow(window.Object)).Returns(workspace.Object);
			workspace
				.Setup(w => w.TryGetWindowLocation(It.IsAny<IWindow>()))
				.Returns(
					new WindowState()
					{
						Location = new Location<int>(),
						Window = window.Object,
						WindowSize = WindowSize.Normal,
					}
				);
			workspace.Setup(w => w.ActiveLayoutEngine).Returns(engine);
			InnerLayoutEngine.Setup(ile => ile.ContainsWindow(window.Object)).Returns(false);
			InnerLayoutEngine.Setup(ile => ile.RemoveWindow(window.Object)).Returns(InnerLayoutEngine.Object);

			return this;
		}
	}

	#region Add
	[Fact]
	public void Add_UseInner()
	{
		// Given
		Wrapper wrapper = new();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);
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
		Mock<IWindow> window = new();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);

		// When
		wrapper.Setup_PluginTracksLocation(new Mock<IWorkspace>(), window, engine).Setup_GetMonitorAtPoint();
		wrapper.FloatingLayoutPlugin.MarkWindowAsFloating(window.Object);
		ILayoutEngine result = engine.AddWindow(window.Object);

		// Then
		Assert.Same(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void Add_AlreadyTrackedByFloatingLayoutEngine_LocationHasNotChanged()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
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
			.Setup_PluginTracksLocation(workspace, window, wrapper.InnerLayoutEngine.Object);

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);

		// When
		wrapper.FloatingLayoutPlugin.MarkWindowAsFloating(window.Object);
		ILayoutEngine result = engine.AddWindow(window.Object);
		ILayoutEngine result2 = result.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.Same(result, result2);

		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Exactly(2));
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Exactly(3));
	}
	#endregion

	#region MoveWindowToPoint
	[Fact]
	public void MoveWindowToPoint_UseInner()
	{
		// Given
		Wrapper wrapper = new();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window.Object, new Point<double>());

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(
			x => x.MoveWindowToPoint(window.Object, It.IsAny<IPoint<double>>()),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowToPoint_UpdateLocation()
	{
		// Given
		Wrapper wrapper = new Wrapper()
			.Setup_GetMonitorAtPoint()
			.Setup_DwmGetWindowLocation_NotNull(new Location<int>());

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);

		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper.Setup_PluginTracksLocation(workspace, window, engine);

		// When
		wrapper.FloatingLayoutPlugin.MarkWindowAsFloating(window.Object);
		ILayoutEngine result = engine.MoveWindowToPoint(window.Object, new Point<double>());

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.InnerLayoutEngine.Verify(
			x => x.MoveWindowToPoint(window.Object, It.IsAny<IPoint<double>>()),
			Times.Never
		);
		Assert.Single(wrapper.FloatingLayoutPlugin.FloatingWindows);
	}
	#endregion

	#region Remove
	[Fact]
	public void Remove_UseInner()
	{
		// Given
		Wrapper wrapper = new();

		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Remove_TrackedByFloatingLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWorkspace> workspace = new();
		Mock<IWindow> window = new();

		wrapper
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
			new(wrapper.Context.Object, wrapper.FloatingLayoutPlugin, wrapper.InnerLayoutEngine.Object);
		wrapper.Setup_PluginTracksLocation(workspace, window, engine);

		// When
		wrapper.FloatingLayoutPlugin.MarkWindowAsFloating(window.Object);
		ILayoutEngine result = engine.AddWindow(window.Object).RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, result);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		wrapper.MonitorManager.Verify(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>()), Times.Exactly(2));
		wrapper.InnerLayoutEngine.Verify(x => x.RemoveWindow(window.Object), Times.Once);
		Assert.Empty(wrapper.FloatingLayoutPlugin.FloatingWindows);
		Assert.False(result.ContainsWindow(window.Object));
	}

	#endregion

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Mock<IWindow> window2 = new();

		wrapper
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
				wrapper.FloatingLayoutPlugin,
				new ColumnLayoutEngine(new LayoutEngineIdentity())
			);

		wrapper.Setup_PluginTracksLocation(new Mock<IWorkspace>(), window, engine);
		wrapper.FloatingLayoutPlugin.MarkWindowAsFloating(window.Object);

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
}
