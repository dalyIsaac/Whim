using Moq;
using Windows.Win32.Foundation;

namespace Whim.FloatingLayout.Tests;

internal class Wrapper
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
