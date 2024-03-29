namespace Whim;

/// <summary>
/// Handles events from the <see cref="WindowManager"/> for the <see cref="Butler"/>.
/// </summary>
internal interface IButlerEventHandlers
{
	int MonitorsChangedDelay { get; }
	bool AreMonitorsChanging { get; }
	void OnWindowAdded(WindowEventArgs args);
	void OnWindowRemoved(WindowEventArgs args);
	void OnWindowFocused(WindowFocusedEventArgs args);
	void OnWindowMinimizeStart(WindowEventArgs args);
	void OnWindowMinimizeEnd(WindowEventArgs args);
	void OnMonitorsChanged(MonitorsChangedEventArgs args);
}
