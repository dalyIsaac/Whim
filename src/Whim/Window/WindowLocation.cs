namespace Whim;

public class WindowLocation : IWindowLocation
{
	public ILocation Location { get; set; }

	public WindowState WindowState { get; set; }

	public IWindow Window { get; }

	public WindowLocation(IWindow window, ILocation location, WindowState windowState)
	{
		Window = window;
		Location = location;
		WindowState = windowState;
	}
}
