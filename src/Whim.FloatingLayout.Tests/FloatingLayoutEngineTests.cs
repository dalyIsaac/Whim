using Moq;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutEngineTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IImmutableLayoutEngine> InnerLayoutEngine { get; } = new();
		public Mock<IWindow> Window { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IInternalFloatingLayoutPlugin> InternalFloatingLayoutPlugin { get; } = new();
		public Mock<IFloatingLayoutPlugin> Plugin { get; }

		public Wrapper()
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);

			NativeManager.Setup(x => x.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(new Location<int>());

			MonitorManager.Setup(x => x.GetMonitorAtPoint(It.IsAny<ILocation<int>>())).Returns(Monitor.Object);

			Monitor.Setup(x => x.WorkingArea).Returns(new Location<int>() { Width = 1, Height = 1 });

			Plugin = InternalFloatingLayoutPlugin.As<IFloatingLayoutPlugin>();
		}
	}

	[Fact]
	public void AddAtPoint_Add()
	{
		// Given
		Wrapper wrapper = new();
		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		engine.AddAtPoint(window.Object, point);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.AddAtPoint(window.Object, point), Times.Once);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void AddAtPoint_AlreadyAdded()
	{
		// Given
		Wrapper wrapper = new();
		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		engine.AddAtPoint(window.Object, point);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.AddAtPoint(window.Object, point), Times.Never);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new();
		ImmutableFloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		Mock<IWindow> floatingWindow = new();
		Mock<IWindow> dockedWindow = new();

		Mock<IMonitor> monitor = new();

		wrapper.InnerLayoutEngine
			.Setup(x => x.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()))
			.Returns(
				new List<IWindowState>
				{
					new WindowState()
					{
						Window = dockedWindow.Object,
						Location = new Location<int>(),
						WindowSize = WindowSize.Normal
					}
				}
			);

		// When
		IEnumerable<IWindowState> result = engine.DoLayout(new Location<int>(), monitor.Object).ToArray();

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Once);
		Assert.Equal(2, result.Count());
	}
}
