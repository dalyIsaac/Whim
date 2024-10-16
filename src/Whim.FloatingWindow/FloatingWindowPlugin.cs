using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace Whim.FloatingWindow;

/// <inheritdoc />
/// <summary>
/// Creates a new instance of the floating window plugin.
/// </summary>
/// <param name="context"></param>
public class FloatingWindowPlugin(IContext context) : IFloatingWindowPlugin, IInternalFloatingWindowPlugin
{
	private readonly IContext _context = context;

	/// <summary>
	/// <c>whim.floating_window</c>
	/// </summary>
	public string Name => "whim.floating_window";

	/// <inheritdoc />
	public Dictionary<HWND, WindowFloatingState> WindowFloatingStates { get; } = [];

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.Store.Dispatch(
			new AddProxyLayoutEngineTransform(layout => new ProxyFloatingLayoutEngine(_context, this, layout))
		);
		_context.Store.WindowEvents.WindowRemoved += WindowEvents_WindowRemoved;
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FloatingWindowCommands(this);

	private void WindowEvents_WindowRemoved(object? sender, WindowEventArgs e) =>
		WindowFloatingStates.Remove(e.Window.Handle);

	/// <inheritdoc />
	public void MarkWindowAsPossiblyRemoved(HWND hwnd)
	{
		WindowFloatingStates[hwnd] = WindowFloatingState.PossiblyRemoved;

		Task.Run(() =>
		{
			Thread.Sleep(1000);
			if (
				WindowFloatingStates.TryGetValue(hwnd, out WindowFloatingState state)
				&& state == WindowFloatingState.PossiblyRemoved
			)
			{
				WindowFloatingStates.Remove(hwnd);
			}
		});
	}

	/// <inheritdoc />
	public bool IsWindowPossiblyRemoved(HWND hwnd) =>
		WindowFloatingStates.TryGetValue(hwnd, out WindowFloatingState state)
		&& state == WindowFloatingState.PossiblyRemoved;

	/// <inheritdoc />
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		window ??= _context.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to mark as floating");
			return;
		}

		if (!_context.Store.Pick(Pickers.PickWindowPosition(window.Handle)).TryGet(out WindowPosition windowPosition))
		{
			Logger.Error($"Could not obtain position for floating window {window}");
			return;
		}

		WindowFloatingStates[window.Handle] = WindowFloatingState.Floating;
		_context.Store.Dispatch(new MoveWindowToPointTransform(window.Handle, windowPosition.LastWindowRectangle));
	}

	/// <inheritdoc />
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		window ??= _context.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to mark as docked");
			return;
		}

		if (!_context.Store.Pick(Pickers.PickWindowPosition(window.Handle)).TryGet(out WindowPosition windowPosition))
		{
			Logger.Error($"Could not obtain position for docked window {window}");
			return;
		}

		if (WindowFloatingStates.Remove(window.Handle))
		{
			_context.Store.Dispatch(new MoveWindowToPointTransform(window.Handle, windowPosition.LastWindowRectangle));
		}
	}

	/// <inheritdoc />
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _context.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to toggle floating");
			return;
		}

		if (
			WindowFloatingStates.TryGetValue(window.Handle, out WindowFloatingState state)
			&& state == WindowFloatingState.Floating
		)
		{
			MarkWindowAsDocked(window);
		}
		else
		{
			MarkWindowAsFloating(window);
		}
	}

	/// <inheritdoc />
	public void LoadState(JsonElement pluginSavedState) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;
}
