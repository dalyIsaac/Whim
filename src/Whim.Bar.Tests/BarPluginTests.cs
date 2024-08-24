using Microsoft.UI.Dispatching;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

public class BarPluginTests
{
	[Theory, AutoSubstituteData]
	public void MonitorManager_MonitorsChanged_RemovedMonitors(IContext context, IMonitor monitor)
	{
		// Given
		BarConfig barConfig = new([], [], []);
		BarPlugin barPlugin = new(context, barConfig);
		NativeManagerUtils.SetupTryEnqueue(context);

		// When MonitorManager_MonitorsChanged is called with a removed monitor which is not in the map
		barPlugin.PreInitialize();
		context.MonitorManager.MonitorsChanged += Raise.EventWith(
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = [],
				UnchangedMonitors = [],
				RemovedMonitors = [monitor],
			}
		);

		// Then an exception is not thrown.
		barPlugin.Dispose();
		context.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}
}
