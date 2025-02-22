namespace Whim;

/// <summary>
/// Custom logic to immediately minimize any "Meeting compact view" window from Teams.
/// </summary>
public class TeamsWindowProcessor : IWindowProcessor
{
	/// <inheritdoc />
	public IWindow Window { get; }

	private TeamsWindowProcessor(IWindow window)
	{
		Window = window;
	}

	/// <summary>
	/// Creates a new instance of the implementing class, if the given window matches the processor.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	public static IWindowProcessor? Create(IContext ctx, IWindow window) =>
		window.ProcessFileName == "ms-teams.exe" ? new TeamsWindowProcessor(window) : null;

	/// <summary>
	/// If the window title starts with "Meeting compact view", minimize the window.
	/// Otherwise, process the event.
	/// </summary>
	/// <param name="eventType"></param>
	/// <param name="idObject"></param>
	/// <param name="idChild"></param>
	/// <param name="idEventThread"></param>
	/// <param name="dwmsEventTime"></param>
	/// <returns></returns>
	public WindowProcessorResult ProcessEvent(
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	)
	{
		if (Window.Title.StartsWith("Meeting compact view"))
		{
			Window.ShowMinimized();
			return WindowProcessorResult.Ignore;
		}

		return WindowProcessorResult.Process;
	}
}
