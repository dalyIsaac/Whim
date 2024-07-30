namespace Whim.FloatingLayout;

/// <summary>
/// ProxyFloatingLayoutPlugin lets windows escape the layout engine and be free-floating.
/// </summary>
public interface IProxyFloatingLayoutPlugin : IPlugin
{
	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null);

	/// <summary>
	/// Mark the given <paramref name="window"/> as a docked window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null);

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null);
}
