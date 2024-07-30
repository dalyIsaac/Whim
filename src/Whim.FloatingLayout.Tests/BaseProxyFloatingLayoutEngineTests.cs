using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;

namespace Whim.FloatingLayout.Tests;

public class BaseProxyFloatingLayoutEngineTests : ProxyLayoutEngineBaseTests
{
	public override Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine =>
		(inner) =>
		{
			IContext context = Substitute.For<IContext>();
			IMonitor monitor = Substitute.For<IMonitor>();
			IInternalProxyFloatingLayoutPlugin plugin = Substitute.For<IInternalProxyFloatingLayoutPlugin>();
			ILayoutEngine innerLayoutEngine = Substitute.For<ILayoutEngine>();

			context
				.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>())
				.Returns(new Rectangle<int>() { Width = 100, Height = 100 });
			context.MonitorManager.GetMonitorAtPoint(Arg.Any<IRectangle<int>>()).Returns(monitor);
			monitor.WorkingArea.Returns(new Rectangle<int>() { Width = 1000, Height = 1000 });
			innerLayoutEngine.Identity.Returns(new LayoutEngineIdentity());

			return new ProxyFloatingLayoutEngine(context, plugin, innerLayoutEngine);
		};
}
