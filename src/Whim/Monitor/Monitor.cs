namespace Whim;

/// <summary>
/// Implementation of <see cref="IMonitor"/>.
/// </summary>
public class Monitor : IMonitor
{
	/// <summary>
	/// Internal representation of a screen, based on the WinForms Screen class.
	/// This has been ported to <see cref="Screen"/>.
	/// </summary>
	internal Screen Screen { get; }

	/// <summary>
	///
	/// </summary>
	/// <param name="screen"></param>
	internal Monitor(Screen screen)
	{
		Screen = screen;
	}

	public string Name => Screen.DeviceName;
	public int Width => Screen.WorkingArea.Width;
	public int Height => Screen.WorkingArea.Height;
	public int X => Screen.WorkingArea.X;
	public int Y => Screen.WorkingArea.Y;
	public bool IsPrimary => Screen.Primary;

	public bool IsPointInside(IPoint<int> point) => ILocation<int>.IsPointInside(this, point);

	public override string ToString() => Screen.ToString();
}
