#nullable enable
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"

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
using Whim.TreeLayout.CommandPalette;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

/// <summary>
/// This is what's called when Whim is loaded.
/// </summary>
/// <param name="context"></param>
void DoConfig(IContext context)
{
	context.Logger.Config = new LoggerConfig();

	context.WorkspaceManager.CreateLayoutEngines = () => new ILayoutEngine[]
	{
		new TreeLayoutEngine(context),
		new ColumnLayoutEngine()
	};

	// Add workspaces.
	context.WorkspaceManager.Add("1");
	context.WorkspaceManager.Add("2");
	context.WorkspaceManager.Add("3");
	context.WorkspaceManager.Add("4");

	// Bar plugin.
	List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
	List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
	List<BarComponent> rightComponents = new()
	{
		ActiveLayoutWidget.CreateComponent(),
		DateTimeWidget.CreateComponent()
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
	FocusIndicatorConfig focusIndicatorConfig = new() { Color = new SolidColorBrush(Colors.Red), FadeEnabled = true };
	FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
	context.PluginManager.AddPlugin(focusIndicatorPlugin);

	// Command palette.
	CommandPaletteConfig commandPaletteConfig = new(context);
	CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
	context.PluginManager.AddPlugin(commandPalettePlugin);

	// Tree layout.
	TreeLayoutPlugin treeLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(treeLayoutPlugin);

	// Tree layout bar.
	TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
	context.PluginManager.AddPlugin(treeLayoutBarPlugin);
	rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

	// Tree layout command palette.
	TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin = new(context, treeLayoutPlugin, commandPalettePlugin);
	context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);
}

#pragma warning disable CS8974 // Methods should not return 'this'.
// We return doConfig here so that Whim can call it when it loads.
return DoConfig;
#pragma warning restore CS8974 // Methods should not return 'this'.
