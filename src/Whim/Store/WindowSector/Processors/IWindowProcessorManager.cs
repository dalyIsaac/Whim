namespace Whim;

/// <summary>
/// The container for custom window processors.
/// These are used to handle windows with non-standard behavior.
/// For example, Firefox will try reset the window position on startup. The <see cref="FirefoxWindowProcessor"/>
/// will ignore these events.
/// </summary>
public interface IWindowProcessorManager
{
	/// <summary>
	/// All the processors that are currently registered.
	/// By default, this will contain the <see cref="FirefoxWindowProcessor"/> and the <see cref="TeamsWindowProcessor"/>.
	/// </summary>
	IDictionary<string, Func<IContext, IWindow, IWindowProcessor?>> ProcessorCreators { get; }

	/// <summary>
	/// Checks if the given window should be ignored by Whim.
	/// </summary>
	/// <param name="window">The window to check.</param>
	/// <param name="eventType">The event type.</param>
	/// <param name="idObject">The object ID.</param>
	/// <param name="idChild">The child ID.</param>
	/// <param name="idEventThread">The event thread ID.</param>
	/// <param name="dwmsEventTime">The event time.</param>
	/// <returns>True if the window should be ignored; otherwise, false.</returns>
	bool ShouldBeIgnored(
		IWindow window,
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	);
}
