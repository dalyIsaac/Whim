using System.Windows.Forms;

namespace Whim.Core;

/// <summary>
/// Implementation of <see cref="IMonitor"/>.
/// </summary>
public class Monitor : IMonitor
{
	/// <summary>
	/// Internal WinForms <see cref="System.Windows.Forms.Screen"/>.
	/// </summary>
	internal Screen Screen { get; }

	/// <summary>
	///
	/// </summary>
	/// <param name="screen">
	/// Internal WinForms <see cref="System.Windows.Forms.Screen"/> from which
	/// <see cref="Monitor"/> exposes information.
	/// </param>
	public Monitor(Screen screen)
	{
		Screen = screen;
	}

	public string Name => Screen.DeviceName;
	public int Width => Screen.WorkingArea.Width;
	public int Height => Screen.WorkingArea.Height;
	public int X => Screen.WorkingArea.X;
	public int Y => Screen.WorkingArea.Y;
	public bool IsPrimary => Screen.Primary;

	public bool IsPointInside(int x, int y) => Location.IsPointInside(this, x, y);
}
