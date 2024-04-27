using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Try get the window for a given <paramref name="Handle"/>.
/// </summary>
/// <param name="Handle"></param>
public record TryGetWindowPicker(HWND Handle) : Picker<Result<IWindow>>
{
	internal override Result<IWindow> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		return rootSector.Windows.Windows.TryGetValue(Handle, out IWindow? window)
			? Result.FromValue(window)
			: Result.FromException<IWindow>(new WhimException($"Could not find window with handle {Handle}"));
	}
}
