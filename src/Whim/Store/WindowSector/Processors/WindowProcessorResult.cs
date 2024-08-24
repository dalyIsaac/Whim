namespace Whim;

/// <summary>
/// The result of processing a window event by a <see cref="IWindowProcessor"/>.
/// </summary>
public enum WindowProcessorResult
{
	/// <summary>
	/// The event should be ignored.
	/// </summary>
	Ignore,

	/// <summary>
	/// The event should be ignored, and the active workspaces should be laid out.
	/// </summary>
	IgnoreAndLayout,

	/// <summary>
	/// The event should be processed.
	/// </summary>
	Process,

	/// <summary>
	/// The event should be processed and the processor should be removed.
	/// </summary>
	ProcessAndRemove,
}
