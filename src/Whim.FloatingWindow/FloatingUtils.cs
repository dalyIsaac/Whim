using System.Collections.Immutable;
using DotNext;

namespace Whim.FloatingWindow;

/// <summary>
/// Provide methods for the floating engines
/// </summary>
internal static class FloatingUtils
{
	/// <summary>
	/// Update the position of a <paramref name="window"/> from the <paramref name="dict"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="dict"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	public static ImmutableDictionary<IWindow, IRectangle<double>>? UpdateWindowRectangle(
		IContext context,
		ImmutableDictionary<IWindow, IRectangle<double>> dict,
		IWindow window
	)
	{
		// Try get the old rectangle.
		IRectangle<double>? oldRectangle = dict.TryGetValue(window, out IRectangle<double>? rectangle)
			? rectangle
			: null;

		// Since the window is floating, we update the rectangle, and return.
		IRectangle<int>? newActualRectangle = context.NativeManager.DwmGetWindowRectangle(window.Handle);
		if (newActualRectangle == null)
		{
			Logger.Error($"Could not obtain rectangle for floating window {window}");
			return null;
		}

		Result<IMonitor> newMonitorResult = context.Store.Pick(Pickers.PickMonitorByWindow(window.Handle));
		if (!newMonitorResult.TryGet(out IMonitor newMonitor))
		{
			Logger.Error($"Could not obtain monitor for floating window {window}");
			return null;
		}

		IRectangle<double> newUnitSquareRectangle = newMonitor.WorkingArea.NormalizeRectangle(newActualRectangle);
		if (newUnitSquareRectangle.Equals(oldRectangle))
		{
			Logger.Debug($"Rectangle for window {window} has not changed");
			return dict;
		}

		ImmutableDictionary<IWindow, IRectangle<double>> newDict = dict.SetItem(window, newUnitSquareRectangle);

		return newDict;
	}
}
