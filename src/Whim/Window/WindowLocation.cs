using System;

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

	// override object.Equals
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is WindowLocation location &&
			   Location.Equals(location.Location) &&
			   WindowState == location.WindowState &&
			   Window.Equals(location.Window);
	}

	// override object.GetHashCode
	public override int GetHashCode() => HashCode.Combine(Location, WindowState, Window);
}
