namespace Whim.SliceLayout;

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
	WindowInsertionType WindowInsertionType { get; set; }
}
