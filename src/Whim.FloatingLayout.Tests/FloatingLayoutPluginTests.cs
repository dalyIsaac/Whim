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
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public ImmutableFloatingLayoutEngine FloatingLayoutEngine { get; }
		public FloatingLayoutPlugin Plugin;

		public Wrapper()
		{
			Context.SetupGet(x => x.WindowManager).Returns(WindowManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);
			NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((ILocation<int>?)null);

			Plugin = new(Context.Object);
			FloatingLayoutEngine = new(Context.Object, Plugin, new Mock<ILayoutEngine>().Object);
		}
	}

	[Fact]
	public void Name()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

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
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WorkspaceManager.Verify(x => x.AddProxyLayoutEngine(It.IsAny<CreateProxyLayoutEngine>()), Times.Once);
	}

	[Fact]
	public void SaveState()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutPlugin plugin = wrapper.Plugin;

		// When
		JsonElement? json = plugin.SaveState();

		// Then
		Assert.Null(json);
	}
}
