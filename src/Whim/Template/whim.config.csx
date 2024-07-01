#nullable enable
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "WHIM_PATH\plugins\Whim.SliceLayout\Whim.SliceLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.Updater\Whim.Updater.dll"

using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;
using Whim.TreeLayout.CommandPalette;
using Whim.Updater;
using Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// This is what's called when Whim is loaded.
/// </summary>
/// <param name="context"></param>
void DoConfig(IContext context)
{
	context.Logger.Config = new LoggerConfig();

	// Bar plugin.
	List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
	List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
	List<BarComponent> rightComponents =
		new()
		{
			BatteryWidget.CreateComponent(),
			ActiveLayoutWidget.CreateComponent(),
			DateTimeWidget.CreateComponent()
		};

	BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
	BarPlugin barPlugin = new(context, barConfig);
	context.PluginManager.AddPlugin(barPlugin);

	// Gap plugin.
	GapsConfig gapsConfig = new() { OuterGap = 0, InnerGap = 10 };
	GapsPlugin gapsPlugin = new(context, gapsConfig);
	context.PluginManager.AddPlugin(gapsPlugin);

	// Floating window plugin.
	FloatingLayoutPlugin floatingLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(floatingLayoutPlugin);

	// Focus indicator.
	FocusIndicatorConfig focusIndicatorConfig = new() { Color = new SolidColorBrush(Colors.Red), FadeEnabled = true };
	FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
	context.PluginManager.AddPlugin(focusIndicatorPlugin);

	// Command palette.
	CommandPaletteConfig commandPaletteConfig = new(context);
	CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
	context.PluginManager.AddPlugin(commandPalettePlugin);

	// Slice layout.
	SliceLayoutPlugin sliceLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(sliceLayoutPlugin);

	// Tree layout.
	TreeLayoutPlugin treeLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(treeLayoutPlugin);

	// Tree layout bar.
	TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
	context.PluginManager.AddPlugin(treeLayoutBarPlugin);
	rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

	// Tree layout command palette.
	TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin =
		new(context, treeLayoutPlugin, commandPalettePlugin);
	context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);

	// Layout preview.
	LayoutPreviewPlugin layoutPreviewPlugin = new(context);
	context.PluginManager.AddPlugin(layoutPreviewPlugin);

	// Updater.
	UpdaterConfig updaterConfig = new() { ReleaseChannel = ReleaseChannel.Alpha };
	UpdaterPlugin updaterPlugin = new(context, updaterConfig);
	context.PluginManager.AddPlugin(updaterPlugin);

	// Set up workspaces.
	context.WorkspaceManager.Add("1");
	context.WorkspaceManager.Add("2");
	context.WorkspaceManager.Add("3");
	context.WorkspaceManager.Add("4");

	// Set up layout engines.
	context.WorkspaceManager.CreateLayoutEngines = () =>
		new CreateLeafLayoutEngine[]
		{
			(id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
			(id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
			(id) => SliceLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id),
			(id) => new FocusLayoutEngine(id),
			(id) => new TreeLayoutEngine(context, treeLayoutPlugin, id),
			(id) => new FreeLayoutEngine(context, id)
		};
}

// We return doConfig here so that Whim can call it when it loads.
return DoConfig;
