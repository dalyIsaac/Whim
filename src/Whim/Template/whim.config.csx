#nullable enable
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"

using System;
using System.Collections.Generic;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;
using Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// Returns a new workspace with <paramref name="name"/> and layout engines.
/// </summary>
/// <param name="context"></param>
/// <param name="name"></param>
/// <returns></returns>
IWorkspace CreateWorkspace(IContext context, string name)
{
	return IWorkspace.CreateWorkspace(
		context,
		name,
		new TreeLayoutEngine(context),
		new ColumnLayoutEngine(),
		new ColumnLayoutEngine("Right to left", false));
}

/// <summary>
/// This is what's called when Whim is loaded.
/// </summary>
/// <param name="context"></param>
void DoConfig(IContext context)
{
	context.WorkspaceManager.WorkspaceFactory = CreateWorkspace;

	// Add workspaces.
	context.WorkspaceManager.Add(CreateWorkspace(context, "Work"));
	context.WorkspaceManager.Add(CreateWorkspace(context, "Procrastination"));
	context.WorkspaceManager.Add(CreateWorkspace(context, "Entertainment"));

	// Bar plugin.
	List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
	List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
	List<BarComponent> rightComponents = new()
	{
		ActiveLayoutWidget.CreateComponent(),
		DateTimeWidget.CreateComponent(),
		TreeLayoutEngineWidget.CreateComponent()
	};

	BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
	BarPlugin barPlugin = new(context, barConfig);
	context.PluginManager.AddPlugin(barPlugin);

	// Floating window plugin.
	FloatingLayoutPlugin floatingLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(floatingLayoutPlugin);

	// Gap plugin.
	GapsConfig gapsConfig = new() { OuterGap = 0, InnerGap = 10 };
	GapsPlugin gapsPlugin = new(context, gapsConfig);
	context.PluginManager.AddPlugin(gapsPlugin);

	// Focus indicator.
	FocusIndicatorConfig focusIndicatorConfig = new() { FadeEnabled = true };
	FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
	context.PluginManager.AddPlugin(focusIndicatorPlugin);

	// Command palette.
	CommandPaletteConfig commandPaletteConfig = new(context);
	CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
	context.PluginManager.AddPlugin(commandPalettePlugin);

	// Tree layout.
	TreeLayoutPlugin treeLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(treeLayoutPlugin);
}

#pragma warning disable CS8974 // Methods should not return 'this'.
// We return doConfig here so that Whim can call it when it loads.
return DoConfig;
#pragma warning restore CS8974 // Methods should not return 'this'.
