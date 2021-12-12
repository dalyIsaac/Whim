namespace Whim.Core.Location;

public class WindowLocation : IWindowLocation
{
	public ILocation? Location { get; set; }

	public IWindow Window { get; }

	public WindowLocation(IWindow window)
	{
		Window = window;
	}

	public WindowLocation(IWindow window, ILocation location)
	{
		Window = window;
		Location = location;
	}
}
