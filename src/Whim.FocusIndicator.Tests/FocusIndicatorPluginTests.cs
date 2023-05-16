using Moq;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FocusIndicator.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FocusIndicatorPluginTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IWindow> Window { get; } = new();
		public FocusIndicatorConfig FocusIndicatorConfig { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.WindowManager).Returns(WindowManager.Object);

			WorkspaceManager.SetupGet(x => x.ActiveWorkspace).Returns(Workspace.Object);

			WindowManager.Setup(x => x.CreateWindow(It.IsAny<HWND>())).Returns(Window.Object);
		}
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		FocusIndicatorPlugin plugin = new(wrapper.Context.Object, wrapper.FocusIndicatorConfig);

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WindowManager.VerifyAdd(x => x.WindowFocused += It.IsAny<EventHandler<WindowEventArgs>>(), Times.Once);
		wrapper.WindowManager.VerifyAdd(x => x.WindowAdded += It.IsAny<EventHandler<WindowEventArgs>>(), Times.Once);
		wrapper.WindowManager.VerifyAdd(x => x.WindowRemoved += It.IsAny<EventHandler<WindowEventArgs>>(), Times.Once);
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMoveStart += It.IsAny<EventHandler<WindowEventArgs>>(),
			Times.Once
		);
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMinimizeStart += It.IsAny<EventHandler<WindowEventArgs>>(),
			Times.Once
		);
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMinimizeEnd += It.IsAny<EventHandler<WindowEventArgs>>(),
			Times.Once
		);
	}

	private static void WindowManager_EventSink_Show(Action<Wrapper> action)
	{
		// Given
		Wrapper wrapper = new();
		FocusIndicatorPlugin plugin = new(wrapper.Context.Object, wrapper.FocusIndicatorConfig);
		plugin.PreInitialize();

		// When
		action(wrapper);

		// Then
		Assert.True(plugin.IsVisible);
	}

	private static void WindowManager_EventSink_Hide(Action<Wrapper> action)
	{
		// Given
		Wrapper wrapper = new();
		FocusIndicatorPlugin plugin = new(wrapper.Context.Object, wrapper.FocusIndicatorConfig);
		plugin.PreInitialize();

		// When
		action(wrapper);

		// Then
		Assert.False(plugin.IsVisible);
	}

	[Fact]
	public void WindowManager_WindowFocused()
	{
		WindowManager_EventSink_Show(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowFocused += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}

	[Fact]
	public void WindowManager_WindowAdded()
	{
		WindowManager_EventSink_Show(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowAdded += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}

	[Fact]
	public void WindowManager_WindowRemoved()
	{
		WindowManager_EventSink_Show(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowRemoved += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}

	[Fact]
	public void WindowManager_WindowMoveStart()
	{
		WindowManager_EventSink_Hide(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowMoveStart += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}

	[Fact]
	public void WindowManager_WindowMinimizeStart()
	{
		WindowManager_EventSink_Hide(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowMinimizeStart += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}

	[Fact]
	public void WindowManager_WindowMinimizeEnd()
	{
		WindowManager_EventSink_Show(
			(wrapper) =>
				wrapper.WindowManager.Raise(
					x => x.WindowMinimizeEnd += null,
					wrapper.WindowManager.Object,
					new WindowEventArgs() { Window = wrapper.Window.Object }
				)
		);
	}
}
