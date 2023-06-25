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
		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();
		public Mock<IWindow> Window { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);

			NativeManager.Setup(x => x.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(new Location<int>());

			MonitorManager.Setup(x => x.GetMonitorAtPoint(It.IsAny<ILocation<int>>())).Returns(Monitor.Object);

			Monitor.Setup(x => x.WorkingArea).Returns(new Location<int>() { Width = 1, Height = 1 });
		}
	}

	[Fact]
	public void AddWindowAtPoint_Add()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		engine.AddWindowAtPoint(window.Object, point);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindowAtPoint(window.Object, point), Times.Once);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void AddWindowAtPoint_AlreadyAdded()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		engine.MarkWindowAsFloating(window.Object);
		wrapper.InnerLayoutEngine.Invocations.Clear();
		wrapper.NativeManager.Invocations.Clear();
		engine.AddWindowAtPoint(window.Object, point);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.AddWindowAtPoint(window.Object, point), Times.Never);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloating()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		engine.MarkWindowAsFloating(window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.Remove(window.Object), Times.Once);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloating_NoWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.MarkWindowAsFloating();

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.Remove(It.IsAny<IWindow>()), Times.Never);
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void MarkWindowAsDocked_WindowIsNotFloating()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		engine.MarkWindowAsDocked(window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.Add(window.Object), Times.Never);
	}

	[Fact]
	public void MarkWindowAsDocked_Success()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		wrapper.InnerLayoutEngine.Setup(x => x.Remove(window.Object)).Returns(true);

		// When
		engine.MarkWindowAsFloating(window.Object);
		wrapper.NativeManager.Invocations.Clear();
		engine.MarkWindowAsDocked(window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(
			x => x.AddWindowAtPoint(window.Object, It.IsAny<IPoint<double>>()),
			Times.Once
		);
		wrapper.NativeManager.Verify(expression: x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void MarkWindowAsDocked_NoWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.MarkWindowAsDocked();

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.Add(It.IsAny<IWindow>()), Times.Never);
		wrapper.NativeManager.Verify(expression: x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void ToggleWindowFloating_FloatingToDocked()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();
		wrapper.InnerLayoutEngine.Setup(x => x.Remove(window.Object)).Returns(true);

		// When
		engine.MarkWindowAsFloating(window.Object);
		wrapper.InnerLayoutEngine.Invocations.Clear();
		wrapper.NativeManager.Invocations.Clear();
		engine.ToggleWindowFloating(window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(
			x => x.AddWindowAtPoint(window.Object, It.IsAny<IPoint<double>>()),
			Times.Once
		);

		wrapper.InnerLayoutEngine.Verify(x => x.Remove(window.Object), Times.Never);
		wrapper.NativeManager.Verify(expression: x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void ToggleWindowFloating_DockedToFloating()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		engine.ToggleWindowFloating(window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(
			x => x.AddWindowAtPoint(window.Object, It.IsAny<IPoint<double>>()),
			Times.Never
		);

		wrapper.InnerLayoutEngine.Verify(x => x.Remove(window.Object), Times.Once);
		wrapper.NativeManager.Verify(expression: x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void ToggleWindowFloating_NoWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.ToggleWindowFloating();

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.Add(It.IsAny<IWindow>()), Times.Never);
		wrapper.InnerLayoutEngine.Verify(x => x.Remove(It.IsAny<IWindow>()), Times.Never);
		wrapper.NativeManager.Verify(expression: x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
	}

	[Fact]
	public void UpdateWindowLocation_Success()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// When
		engine.MarkWindowAsFloating(window.Object);
		wrapper.NativeManager.Invocations.Clear();
		engine.UpdateWindowLocation(window.Object);

		// Then
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void UpdateWindowLocation_CouldNotGetLocation()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);
		Mock<IWindow> window = new();

		// Mark the window as floating, and reset the mocks
		engine.MarkWindowAsFloating(window.Object);
		wrapper.NativeManager.Invocations.Clear();
		wrapper.NativeManager.Setup(x => x.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((ILocation<int>?)null);

		// When
		engine.UpdateWindowLocation(window.Object);

		// Then
		wrapper.NativeManager.Verify(x => x.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine = new(wrapper.Context.Object, wrapper.InnerLayoutEngine.Object);

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
		engine.MarkWindowAsFloating(floatingWindow.Object);
		IEnumerable<IWindowState> result = engine.DoLayout(new Location<int>(), monitor.Object).ToArray();

		// Then
		wrapper.InnerLayoutEngine.Verify(x => x.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Once);
		Assert.Equal(2, result.Count());
	}
}
