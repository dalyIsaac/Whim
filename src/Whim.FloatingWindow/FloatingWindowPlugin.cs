using System.Collections.Generic;
using System.Text.Json;
using Windows.Win32.Foundation;

namespace Whim.FloatingWindow;

/// <inheritdoc />
/// <summary>
/// Creates a new instance of the floating window plugin.
/// </summary>
/// <param name="context"></param>
public class FloatingWindowPlugin(IContext context) : IFloatingWindowPlugin
{
	private readonly IContext _ctx = context;

	/// <summary>
	/// <c>whim.floating_window</c>
	/// </summary>
	public string Name => "whim.floating_window";

	private readonly HashSet<HWND> _floatingWindows = [];

	/// <inheritdoc />
	public IReadOnlySet<HWND> FloatingWindows => _floatingWindows;

	/// <inheritdoc />
	public void PreInitialize()
	{
		_ctx.Store.Dispatch(
			new AddProxyLayoutEngineTransform(layout => new ProxyFloatingLayoutEngine(_ctx, this, layout))
		);
		_ctx.Store.WindowEvents.WindowRemoved += WindowEvents_WindowRemoved;
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FloatingWindowCommands(this);

	private void WindowEvents_WindowRemoved(object? sender, WindowEventArgs e) =>
		_floatingWindows.Remove(e.Window.Handle);

	/// <inheritdoc />
	public void MarkWindowAsFloating(IWindow? window = null)
	{
		window ??= _ctx.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to mark as floating");
			return;
		}

		if (!_ctx.Store.Pick(Pickers.PickWindowPosition(window.Handle)).TryGet(out WindowPosition windowPosition))
		{
			Logger.Error($"Could not obtain position for floating window {window}");
			return;
		}

		_floatingWindows.Add(window.Handle);
		_ctx.Store.Dispatch(new MoveWindowToPointTransform(window.Handle, windowPosition.LastWindowRectangle));
	}

	/// <inheritdoc />
	public void MarkWindowAsDocked(IWindow? window = null)
	{
		window ??= _ctx.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to mark as docked");
			return;
		}

		if (!_ctx.Store.Pick(Pickers.PickWindowPosition(window.Handle)).TryGet(out WindowPosition windowPosition))
		{
			Logger.Error($"Could not obtain position for docked window {window}");
			return;
		}

		if (_floatingWindows.Remove(window.Handle))
		{
			_ctx.Store.Dispatch(new MoveWindowToPointTransform(window.Handle, windowPosition.LastWindowRectangle));
		}
	}

	/// <inheritdoc />
	public void ToggleWindowFloating(IWindow? window = null)
	{
		window ??= _ctx.Store.Pick(Pickers.PickLastFocusedWindow()).ValueOrDefault;

		if (window == null)
		{
			Logger.Error("No window to toggle floating");
			return;
		}

		if (_floatingWindows.Contains(window.Handle))
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
