namespace Whim;

public class WindowLocation : IWindowLocation
{
	public ILocation<int> Location { get; set; }

	public WindowState WindowState { get; set; }

	public IWindow Window { get; }

	public WindowLocation(IWindow window, ILocation<int> location, WindowState windowState)
	{
		Window = window;
		Location = location;
		WindowState = windowState;
	}
}
