using System;

namespace Whim;

/// <inheritdoc />
public class WindowState : IWindowState
{
	/// <inheritdoc />
	public required ILocation<int> Location { get; set; }

	/// <inheritdoc />
	public required WindowSize WindowSize { get; set; }

	/// <inheritdoc />
	public required IWindow Window { get; init; }

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

		return obj is WindowState location
			&& Location.Equals(location.Location)
			&& WindowSize == location.WindowSize
			&& Window.Equals(location.Window);
	}

	/// <inheritdoc />
	public static bool operator ==(WindowState? left, WindowState? right) => Equals(left, right);

	/// <inheritdoc />
	public static bool operator !=(WindowState? left, WindowState? right) => !Equals(left, right);

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(Location, WindowSize, Window);
}
