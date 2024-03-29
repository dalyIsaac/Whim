using System.Collections.Generic;
using NSubstitute;

namespace Whim.TestUtils;

public static class MonitorManagerUtils
{
	/// <summary>
	/// Set up the monitors for the <see cref="IMonitorManager"/> to have the specified monitors.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="monitors"></param>
	/// <param name="activeMonitorIndex"></param>
	public static void SetupMonitors(IContext ctx, IMonitor[] monitors, int activeMonitorIndex = 0)
	{
		ctx.MonitorManager.GetEnumerator().Returns((_) => ((IEnumerable<IMonitor>)monitors).GetEnumerator());
		ctx.MonitorManager.Length.Returns(monitors.Length);
		if (monitors.Length > 0)
		{
			ctx.MonitorManager.ActiveMonitor.Returns(monitors[activeMonitorIndex]);

			ctx.MonitorManager.GetPreviousMonitor(monitors[activeMonitorIndex])
				.Returns(monitors[(activeMonitorIndex - 1).Mod(monitors.Length)]);
			ctx.MonitorManager.GetNextMonitor(monitors[activeMonitorIndex]);
		}
	}
}
