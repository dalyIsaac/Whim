using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Get the currently active monitor.
/// </summary>
public record GetActiveMonitor() : Selector<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, Store store) =>
		ctx.Store.MonitorSlice.Monitors[ctx.Store.MonitorSlice.ActiveMonitorIndex];
}

/// <summary>
/// Get the primary monitor.
/// </summary>
public record GetPrimaryMonitor() : Selector<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, Store store) =>
		ctx.Store.MonitorSlice.Monitors[ctx.Store.MonitorSlice.PrimaryMonitorIndex];
}

/// <summary>
/// Get the last <see cref="IMonitor"/> which received an event sent by Windows which Whim did not ignore.
/// </summary>
public record GetLastWhimActiveMonitor() : Selector<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, Store store) =>
		ctx.Store.MonitorSlice.Monitors[ctx.Store.MonitorSlice.LastWhimActiveMonitorIndex];
}

/// <summary>
/// Get all the <see cref="IMonitor"/>s tracked by Whim.
/// </summary>
public record GetAllMonitors() : Selector<IReadOnlyList<IMonitor>>()
{
	internal override IReadOnlyList<IMonitor> Execute(IContext ctx, IInternalContext internalCtx, Store store) =>
		ctx.Store.MonitorSlice.Monitors;
}
