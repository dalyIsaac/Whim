namespace Whim.SliceLayout;

/// <summary>
/// The type of insertion to use when adding a window to a slice.
/// </summary>
public enum WindowInsertionType
{
	/// <summary>
	/// Swap the window with the existing window in the slice.
	/// </summary>
	Swap,

	/// <summary>
	/// Insert the window into the slice, pushing the existing window down the stack.
	/// </summary>
	Rotate
}

public interface ISliceLayoutPlugin : IPlugin
{
	/// <summary>
	/// The name of the action that promotes a window in the stack to the next-higher slice.
	/// </summary>
	string PromoteActionName { get; }

	/// <summary>
	/// The name of the action that demotes a window in the stack to the next-lower slice.
	/// </summary>
	string DemoteActionName { get; }

	/// <summary>
	/// The type of insertion to use when adding a window to a slice.
	/// </summary>
	WindowInsertionType WindowInsertionType { get; set; }

	/// <summary>
	/// Promotes the given window in the stack.
	/// </summary>
	/// <param name="window">
	/// The window to promote. If <see langword="null"/>, then <see cref="IWorkspace.LastFocusedWindow"/>
	/// is used.
	/// </param>
	void PromoteWindowInStack(IWindow? window = null);

	/// <summary>
	/// Demotes the given window in the stack.
	/// </summary>
	/// <param name="window">
	/// The window to demote. If <see langword="null"/>, then <see cref="IWorkspace.LastFocusedWindow"/>
	/// is used.
	/// </param>
	void DemoteWindowInStack(IWindow? window = null);
}
