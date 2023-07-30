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
		public Mock<IInternalFloatingLayoutPlugin> Plugin { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();

		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			NativeManager
				.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()))
				.Returns(new Location<int>() { Width = 100, Height = 100 });

			MonitorManager.Setup(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>())).Returns(Monitor.Object);

			Monitor.SetupGet(m => m.WorkingArea).Returns(new Location<int>() { Width = 1000, Height = 1000 });

			Plugin.SetupGet(x => x.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

			InnerLayoutEngine.Setup(ile => ile.Identity).Returns(new LayoutEngineIdentity());
		}

		public Wrapper MarkAsFloating(IWindow window)
		{
			Plugin
				.SetupGet(x => x.FloatingWindows)
				.Returns(
					new Dictionary<IWindow, ISet<LayoutEngineIdentity>>
					{
						{
							window,
							new HashSet<LayoutEngineIdentity> { InnerLayoutEngine.Object.Identity }
						}
					}
				);
			return this;
		}

		public Wrapper Setup_RemoveWindow(IWindow window, Mock<ILayoutEngine> newInnerLayoutEngine)
		{
			InnerLayoutEngine.Setup(ile => ile.RemoveWindow(window)).Returns(newInnerLayoutEngine.Object);
			newInnerLayoutEngine.Setup(ile => ile.Identity).Returns(InnerLayoutEngine.Object.Identity);
			return this;
		}

		public Wrapper Setup_AddWindow(IWindow window, Mock<ILayoutEngine> newInnerLayoutEngine)
		{
			InnerLayoutEngine.Setup(ile => ile.AddWindow(window)).Returns(newInnerLayoutEngine.Object);
			newInnerLayoutEngine.Setup(ile => ile.Identity).Returns(InnerLayoutEngine.Object.Identity);
			return this;
		}
	}

	#region AddWindow
	[Fact]
	public void AddWindow_UseInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_Succeed()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_FailOnNoLocation()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_FailOnSameLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_RemoveFromInner()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_AddWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine2 = newEngine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}
	#endregion

	#region RemoveWindow
	[Fact]
	public void RemoveWindow_UseInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void RemoveWindow_FloatingInPlugin()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		wrapper.MarkAsFloating(window.Object);

		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);

		wrapper.InnerLayoutEngine.Invocations.Clear();
		wrapper.Plugin.Invocations.Clear();

		ILayoutEngine newEngine2 = newEngine1.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Never);
		wrapper.Plugin.VerifyGet(x => x.FloatingWindows, Times.Once);
	}
	#endregion

	#region MoveWindowToPoint
	[Fact]
	public void MoveWindowToPoint_UseInner()
	{
		// Given
		Mock<IWindow> window = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNew()
	{
		// Given
		Mock<IWindow> window = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_SameLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_DifferentLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(new Location<int>());
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}
	#endregion
}
