using Moq;
using System.Text.Json;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class FloatingLayoutPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IFloatingLayoutEngine> FloatingLayoutEngine { get; } = new();

		public Wrapper(bool findFloatingLayoutEngine = true)
		{
			Context.SetupGet(x => x.WindowManager).Returns(WindowManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);
			NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((ILocation<int>?)null);

			Workspace
				.Setup(w => w.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>())
				.Returns(findFloatingLayoutEngine ? FloatingLayoutEngine.Object : null);
		}
	}

	[Fact]
	public void Name()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.floating_layout", name);
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WorkspaceManager.Verify(x => x.AddProxyLayoutEngine(It.IsAny<ProxyLayoutEngine>()), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloating()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		IWindow window = Mock.Of<IWindow>();

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		wrapper.FloatingLayoutEngine.Verify(x => x.MarkWindowAsFloating(window), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloating_CouldNotFindLayoutEngine()
	{
		// Given
		Wrapper wrapper = new(findFloatingLayoutEngine: false);
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		IWindow window = Mock.Of<IWindow>();

		// When
		plugin.MarkWindowAsFloating(window);

		// Then
		wrapper.FloatingLayoutEngine.Verify(x => x.MarkWindowAsFloating(window), Times.Never);
	}

	[Fact]
	public void ToggleWindowFloating()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		IWindow window = Mock.Of<IWindow>();

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		wrapper.FloatingLayoutEngine.Verify(x => x.ToggleWindowFloating(window), Times.Once);
	}

	[Fact]
	public void ToggleWindowFloating_CouldNotFindLayoutEngine()
	{
		// Given
		Wrapper wrapper = new(findFloatingLayoutEngine: false);
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		IWindow window = Mock.Of<IWindow>();

		// When
		plugin.ToggleWindowFloating(window);

		// Then
		wrapper.FloatingLayoutEngine.Verify(x => x.ToggleWindowFloating(window), Times.Never);
	}

	[Fact]
	public void SaveState()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}

	[Fact]
	public void WindowManager_WindowMoved_NoWorkspaceForWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		plugin.PreInitialize();

		wrapper.WorkspaceManager.Setup(w => w.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns((IWorkspace?)null);

		// When
		wrapper.WindowManager.Raise(
			wm => wm.WindowMoved += null,
			new WindowEventArgs() { Window = new Mock<IWindow>().Object }
		);

		// Then
		wrapper.Workspace.Verify(x => x.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>(), Times.Never);
		wrapper.FloatingLayoutEngine.Verify(x => x.UpdateWindowLocation(It.IsAny<IWindow>()), Times.Never);
	}

	[Fact]
	public void WindowManager_WindowMoved_NoLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		plugin.PreInitialize();

		wrapper.WorkspaceManager
			.Setup(w => w.GetWorkspaceForWindow(It.IsAny<IWindow>()))
			.Returns(wrapper.Workspace.Object);
		wrapper.Workspace
			.Setup(w => w.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>())
			.Returns((IFloatingLayoutEngine?)null);

		// When
		wrapper.WindowManager.Raise(
			wm => wm.WindowMoved += null,
			new WindowEventArgs() { Window = new Mock<IWindow>().Object }
		);

		// Then
		wrapper.Workspace.Verify(x => x.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>(), Times.Once);
		wrapper.FloatingLayoutEngine.Verify(x => x.UpdateWindowLocation(It.IsAny<IWindow>()), Times.Never);
	}

	[Fact]
	public void WindowManager_WindowMoved()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		plugin.PreInitialize();

		wrapper.WorkspaceManager
			.Setup(w => w.GetWorkspaceForWindow(It.IsAny<IWindow>()))
			.Returns(wrapper.Workspace.Object);
		wrapper.Workspace
			.Setup(w => w.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>())
			.Returns(wrapper.FloatingLayoutEngine.Object);

		// When
		wrapper.WindowManager.Raise(
			wm => wm.WindowMoved += null,
			new WindowEventArgs() { Window = new Mock<IWindow>().Object }
		);

		// Then
		wrapper.Workspace.Verify(x => x.ActiveLayoutEngine.GetLayoutEngine<IFloatingLayoutEngine>(), Times.Once);
		wrapper.FloatingLayoutEngine.Verify(x => x.UpdateWindowLocation(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object);
		plugin.PreInitialize();

		// When
		plugin.Dispose();

		// Then
		wrapper.WindowManager.VerifyRemove(
			wm => wm.WindowMoved -= It.IsAny<EventHandler<WindowEventArgs>>(),
			Times.Once
		);
	}
}
