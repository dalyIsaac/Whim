namespace Whim;

internal interface IInternalButler
{
    /// <summary>
    /// Trigger <see cref="IButler.WindowRouted"/> from elsewhere within Whim's core.
    /// </summary>
    /// <param name="args"></param>
    void TriggerWindowRouted(RouteEventArgs args);

    /// <summary>
    /// Trigger <see cref="IButler.MonitorWorkspaceChanged"/> from elsewhere within Whim's core.
    /// </summary>
    /// <param name="args"></param>
	void TriggerMonitorWorkspaceChanged(MonitorWorkspaceChangedEventArgs args);
}
