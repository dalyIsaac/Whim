using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Get the currently active monitor.
/// </summary>
public record GetActiveMonitorPicker() : Picker<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector) =>
		rootSector.Monitors.Monitors[rootSector.Monitors.ActiveMonitorIndex];
}

/// <summary>
/// Get the primary monitor.
/// </summary>
public record GetPrimaryMonitorPicker() : Picker<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector) =>
		rootSector.Monitors.Monitors[rootSector.Monitors.PrimaryMonitorIndex];
}

/// <summary>
/// Get the last <see cref="IMonitor"/> which received an event sent by Windows which Whim did not ignore.
/// </summary>
public record GetLastWhimActiveMonitorPicker() : Picker<IMonitor>()
{
	internal override IMonitor Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector) =>
		rootSector.Monitors.Monitors[rootSector.Monitors.LastWhimActiveMonitorIndex];
}

/// <summary>
/// Get all the <see cref="IMonitor"/>s tracked by Whim.
/// </summary>
public record GetAllMonitorsPicker() : Picker<IReadOnlyList<IMonitor>>()
{
	internal override IReadOnlyList<IMonitor> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSector
	) => rootSector.Monitors.Monitors;
}
