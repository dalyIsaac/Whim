namespace Whim;

internal interface IInternalMonitorManager
{
	/// <summary>
	/// The last <see cref="IMonitor"/> which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	IMonitor LastWhimActiveMonitor { get; }

	/// <summary>
	/// Try to get the <see cref="IMonitor"/> for the given <paramref name="hmonitor"/>.
	/// </summary>
	/// <param name="hmonitor"></param>
	/// <returns></returns>
	IMonitor? GetMonitorByHandle(HMONITOR hmonitor);

	/// <summary>
	/// Set the <see cref="IMonitorManager.ActiveMonitor"/> to the given <paramref name="monitor"/>.
	/// This is intended to be used for only empty monitors.
	/// </summary>
	/// <param name="monitor"></param>
	void ActivateEmptyMonitor(IMonitor monitor);
}
