using System;

namespace Whim;

internal interface IButlerEventHandlers : IDisposable
{
	bool AreMonitorsChanging { get; }
	void OnWindowAdded(WindowEventArgs args);
	void OnWindowRemoved(WindowEventArgs args);
	void OnWindowFocused(WindowFocusedEventArgs args);
	void OnWindowMinimizeStart(WindowEventArgs args);
	void OnWindowMinimizeEnd(WindowEventArgs args);
	void OnMonitorsChanged(MonitorsChangedEventArgs args);
}
