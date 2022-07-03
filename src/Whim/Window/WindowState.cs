using System;

namespace Whim;

/// <inheritdoc />
public class WindowState : IWindowState
{
	/// <inheritdoc />
	public ILocation<int> Location { get; set; }

	/// <inheritdoc />
	public WindowSize WindowSize { get; set; }

	/// <inheritdoc />
	public IWindow Window { get; }

	/// <summary>
	/// Creates a new <see cref="WindowState"/>.
	/// </summary>
	/// <param name="window">The window in question.</param>
	/// <param name="location">The location of the window.</param>
	/// <param name="windowSize">The size of the window.</param>
	public WindowState(IWindow window, ILocation<int> location, WindowSize windowSize)
	{
		Window = window;
		Location = location;
		WindowSize = windowSize;
	}

	/// <inheritdoc />
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

		return obj is WindowState location &&
			   Location.Equals(location.Location) &&
			   WindowSize == location.WindowSize &&
			   Window.Equals(location.Window);
	}

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(Location, WindowSize, Window);
}
