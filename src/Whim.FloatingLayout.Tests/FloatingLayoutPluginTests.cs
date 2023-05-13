using Moq;
using System.Text.Json;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IFloatingLayoutEngine> FloatingLayoutEngine { get; } = new();
		public FloatingLayoutConfig FloatingLayoutConfig { get; } = new();

		public Wrapper(bool findFloatingLayoutEngine = true)
		{
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
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WorkspaceManager.Verify(x => x.AddProxyLayoutEngine(It.IsAny<ProxyLayoutEngine>()), Times.Once);
	}

	[Fact]
	public void Ctor_DefaultConfig()
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
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);
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
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);
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
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);
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
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);
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
		FloatingLayoutPlugin plugin = new(wrapper.Context.Object, wrapper.FloatingLayoutConfig);

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}
}
