#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"

using System;
using System.Collections.Generic;
using Whim;
using Whim.Bar;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.TreeLayout;
using Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// Methods to simplify some of the creation of the more tedious keybindings.
/// </summary>
private static class KeybindHelpers
{
	public static KeybindEventHandler FocusWindowInDirection(IConfigContext configContext, Direction direction) => (args) =>
	{
		IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
		if (workspace.LastFocusedWindow == null)
		{
			return;
		}

		workspace.ActiveLayoutEngine.FocusWindowInDirection(direction, workspace.LastFocusedWindow);
	};

	public static KeybindEventHandler SwapWindowInDirection(IConfigContext configContext, Direction direction) => (args) =>
	{
		configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(direction);
	};
}

/// <summary>
/// Returns a new workspace with <paramref name="name"/> and layout engines.
/// </summary>
/// <param name="configContext"></param>
/// <param name="name"></param>
/// <returns></returns>
IWorkspace CreateWorkspace(IConfigContext configContext, string name)
{
	return new Workspace(configContext, name, new ColumnLayoutEngine(), new ColumnLayoutEngine("Right to left", false));
}

/// <summary>
/// This is what's called when Whim is loaded.
/// </summary>
/// <param name="configContext"></param>
/// <returns></returns>
IConfigContext DoConfig(IConfigContext configContext)
{
	// Add workspaces.
	configContext.WorkspaceManager.Add(CreateWorkspace(configContext, "1"));
	configContext.WorkspaceManager.Add(CreateWorkspace(configContext, "2"));
	configContext.WorkspaceManager.Add(CreateWorkspace(configContext, "3"));

	// Bar plugin.
	List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
	List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
	List<BarComponent> rightComponents = new() { ActiveLayoutWidget.CreateComponent(), DateTimeWidget.CreateComponent() };

	BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
	BarPlugin barPlugin = new(configContext, barConfig);

	configContext.PluginManager.RegisterPlugin(barPlugin);

	// Floating window plugin.
	FloatingLayoutPlugin floatingLayoutPlugin = new(configContext);
	configContext.PluginManager.RegisterPlugin(floatingLayoutPlugin);

	// Gap plugin.
	GapsConfig gapsConfig = new(outerGap: 0, innerGap: 10);
	GapsPlugin gapsPlugin = new(configContext, gapsConfig);
	configContext.PluginManager.RegisterPlugin(gapsPlugin);

	// Focus indicator.
	FocusIndicatorConfig focusIndicatorConfig = new() { FadeEnabled = true };
	FocusIndicatorPlugin focusIndicatorPlugin = new(configContext, focusIndicatorConfig);
	configContext.PluginManager.RegisterPlugin(focusIndicatorPlugin);

	// Keybindings.
	KeyModifiers winAlt = KeyModifiers.LWin | KeyModifiers.LAlt;
	KeyModifiers winShift = KeyModifiers.LWin | KeyModifiers.LShift;
	KeyModifiers winCtrl = KeyModifiers.LWin | KeyModifiers.LControl;

	// Focus window in direction.
	configContext.KeybindManager.Add(new Keybind(winAlt, VIRTUAL_KEY.VK_LEFT), KeybindHelpers.FocusWindowInDirection(configContext, Direction.Left));
	configContext.KeybindManager.Add(new Keybind(winAlt, VIRTUAL_KEY.VK_RIGHT), KeybindHelpers.FocusWindowInDirection(configContext, Direction.Right));
	configContext.KeybindManager.Add(new Keybind(winAlt, VIRTUAL_KEY.VK_UP), KeybindHelpers.FocusWindowInDirection(configContext, Direction.Up));
	configContext.KeybindManager.Add(new Keybind(winAlt, VIRTUAL_KEY.VK_DOWN), KeybindHelpers.FocusWindowInDirection(configContext, Direction.Down));

	// Swap windows in direction.
	configContext.KeybindManager.Add(new Keybind(winCtrl, VIRTUAL_KEY.VK_LEFT), KeybindHelpers.SwapWindowInDirection(configContext, Direction.Left));
	configContext.KeybindManager.Add(new Keybind(winCtrl, VIRTUAL_KEY.VK_RIGHT), KeybindHelpers.SwapWindowInDirection(configContext, Direction.Right));
	configContext.KeybindManager.Add(new Keybind(winCtrl, VIRTUAL_KEY.VK_UP), KeybindHelpers.SwapWindowInDirection(configContext, Direction.Up));
	configContext.KeybindManager.Add(new Keybind(winCtrl, VIRTUAL_KEY.VK_DOWN), KeybindHelpers.SwapWindowInDirection(configContext, Direction.Down));

	// Move window to monitor.
	configContext.KeybindManager.Add(new Keybind(winShift, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.MoveWindowToPreviousMonitor());
	configContext.KeybindManager.Add(new Keybind(winShift, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.MoveWindowToNextMonitor());

	// Floating layout
	configContext.KeybindManager.Add(new Keybind(winShift, VIRTUAL_KEY.VK_F), (args) => floatingLayoutPlugin.ToggleWindowFloating());

	return configContext;
}

#pragma warning disable CS8974 // Methods should not return 'this'.
// We return doConfig here so that Whim can call it when it loads.
return DoConfig;
#pragma warning restore CS8974 // Methods should not return 'this'.
