namespace Whim;

/// <summary>
/// Handles events from the <see cref="WindowManager"/> for the <see cref="Butler"/>.
/// </summary>
internal interface IButlerEventHandlers
{
	int MonitorsChangedDelay { get; }
	bool AreMonitorsChanging { get; }
	void OnWindowAdded(WindowEventArgs args);
}
